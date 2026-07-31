using System;
using System.Collections.Generic;

public class EffectPipeline : IBattleSystem
{
    private BattleEventBus events;
    private ActorRegistry actors;
    private IStatusEffectDatabase statusDatabase;
    private PositionSystem positions;

    public EffectPipeline(BattleEventBus eventBus, ActorRegistry actorRegistry, IStatusEffectDatabase statusEffectDatabase, PositionSystem positionSystem)
    {
        events = eventBus;
        actors = actorRegistry;
        statusDatabase = statusEffectDatabase;
        positions = positionSystem;

        events.Subscribe<EffectRequestEvent>(OnEffectRequest);
        events.Subscribe<EffectTickRequestEvent>(OnEffectTickRequest);
    }

    public void Update(float deltaTime) { }

    private void OnEffectTickRequest(EffectTickRequestEvent e)
    {
        // Same as normal effect request, but later we can use e.StatusName
        // to skip things like evasion checks
        ProcessEffects(e.Effects, e.Context);
    }

    private void OnEffectRequest(EffectRequestEvent e)
    {
        ProcessEffects(e.Effects, e.Context);
    }

    private void ProcessEffects(List<Effect> effects, EffectContext context)
    {
        var target = actors.GetActor(context.TargetId);
        if (target == null || !target.IsAlive)
        {
            return;
        }

        // 1 // ---- HIT CHECK ----
        // TODO: context.IsHit = CalculateHitChance(context.SourceId, target);
        if (!context.IsHit)
        {
            // The spell completely missed! publish an event for the UI "Miss" and abort
            events.Publish(new PlayerFeedbackEvent("Missed")); // TODO: this should be in floating combat text, not the feedback text
            return;
        }

        // TODO: context.IsCritical = CalculateCritChance(context.SourceId, target);

        // 2 // ---- PROCESS INTERACTIONS ----
        if (!string.IsNullOrEmpty(context.AbilityId))
        {
            for (int i = target.ActiveStatuses.Count - 1; i >= 0; i--)
            {
                var status = target.ActiveStatuses[i];

                if (status.TriggerAbilityId == context.AbilityId)
                {
                    if (!status.RequireMaxStacks || status.Stacks >= status.MaxStacks)
                    {
                        events.Publish(new EffectInteractionEvent(context.SourceId, context.TargetId, status.StatusId, context.AbilityId));

                        if (!string.IsNullOrEmpty(status.TriggerStatusId))
                        {
                            ApplyStatus(new ApplyStatusEffect(status.TriggerStatusId, 0, status.Power), context, target);
                        }

                        if (status.ConsumesStacks)
                        {
                            target.Stats.RemoveModifiersFromSource(status);
                            target.ActiveStatuses.RemoveAt(i);
                            events.Publish(new StatusExpiredEvent(context.TargetId, status.Name));
                        }
                    }
                }
            }
        }

        // 3 // ---- PROCESS STANDARD EFFECTS ----

        bool hasDamageEffect = effects.Exists(e => e is DamageEffect);

        foreach (var effect in effects)
        {
            if (effect is DamageEffect damage)
            {
                ApplyDamage(damage, context, target);
            }
            else if (effect is HealEffect heal)
            {
                ApplyHeal(heal, context, target);
            }
            else if (effect is ApplyStatusEffect status)
            {
                // If ability deals damage, only apply the status if damage > 0
                // if hasDamageEffect == False, then this is a pure debuff like Sleep, so it is still applied
                if (hasDamageEffect && context.FinalDamageDealt <= 0)
                {
                    continue;
                }

                ApplyStatus(status, context, target);
            }
            else if (effect is CleanseEffect cleanse)
            {
                ApplyCleanse(cleanse, context, target);
            }
            else if (effect is ATBModifyEffect atb)
            {
                ApplyATBModification(atb, context, target);
            }
            else if (effect is TeleportToTargetEffect teleport)
            {
                ApplyTeleport(teleport, context, target);
            }
        }
    }

    private void ApplyDamage(DamageEffect damageEffect, EffectContext context, BattleActor target)
    {
        // 1. The RPG Math (Armor, Elemental weakness, Defend State)
        // e.g. int mitigatedDamage = damageEffect.BaseDamage - target.Stats.Armor;
        int baseDamage = damageEffect.BaseDamage;

        if (context.IsCritical)
        {
            baseDamage = (int)(baseDamage * 1.5f);
            // event.Publish floatingText("Critical")
        }
        
        int finalDamage = Math.Max(0, baseDamage);         // ensure we dont heal  the target

        // 2. Apply it to the actor
        target.CurrentHP -= finalDamage;

        // 3. Write to the ledger so subsequent effets know what hapened
        context.FinalDamageDealt = finalDamage;

        // 4. Broadcase the result for UI, Audio, and Debug Logs
        events.Publish(new DamageAppliedEvent(context.SourceId, context.TargetId, finalDamage));

        // 5. Death check
        if (target.CurrentHP <= 0)
        {
            events.Publish(new ActorDiedEvent(context.TargetId));
        }
    }

    private void ApplyHeal(HealEffect healEffect, EffectContext context, BattleActor target)
    {
        // 1. Any heal bonuses applied here
        int finalHeal = Math.Max(0, healEffect.BaseHeal);

        // 2. Apply it
        target.CurrentHP += finalHeal;

        // Any reason that healing would need to be in ledger? Can always add it later
        // context.FinalHealingAmount = finalHeal;

        events.Publish(new HealAppliedEvent(context.SourceId, context.TargetId, finalHeal));
    }

    private void ApplyStatus(ApplyStatusEffect statusEffect, EffectContext context, BattleActor target)
    {
        if (statusDatabase == null)
        {
            return;
        }
        
        // 1. Get the Template from the pure C# Database
        var template = statusDatabase.GetStatusEffect(statusEffect.StatusId);
        if (template == null)
        {
            return;
        }

        // 2. Stacking Rule: Strongest Power/Duration overwrites
        ActiveStatus existingStatus = target.ActiveStatuses.Find(s => s.StatusId == statusEffect.StatusId);

        if (existingStatus != null)
        {
            if (existingStatus.MaxStacks > 1)
            {
                // Increment Stacks
                existingStatus.Stacks++;
                if (existingStatus.Stacks > existingStatus.MaxStacks) 
                {
                    existingStatus.Stacks = existingStatus.MaxStacks;
                }

                // Refresh Duration
                existingStatus.DurationLeft = statusEffect.OverrideDuration > 0 ? statusEffect.OverrideDuration : template.DefaultDuration;

                // --- SELF-TRIGGER --- //
                // triggers automatically at max stacks
                if (string.IsNullOrEmpty(existingStatus.TriggerAbilityId) && existingStatus.Stacks == existingStatus.MaxStacks && !string.IsNullOrEmpty(existingStatus.TriggerStatusId))
                {
                    target.Stats.RemoveModifiersFromSource(existingStatus);
                    target.ActiveStatuses.Remove(existingStatus);
                    events.Publish(new StatusExpiredEvent(context.TargetId, existingStatus.Name));

                    // Self activation = Status reached max stacks. Self-destruct and trigger new status effect
                    // Trigger the new status using the power of the effect being processed (the one that triggered it)
                    ApplyStatus(new ApplyStatusEffect(existingStatus.TriggerStatusId, 0, statusEffect.Power), context, target);
                    return;
                }

            // Refresh and re-apply multipliers based on Stacks
            target.Stats.RemoveModifiersFromSource(existingStatus);
            foreach (var modTemplate in template.StatModifiers)
            {
                var statMod = new StatModifier(modTemplate.FlatValue * existingStatus.Stacks, modTemplate.PercentValue * existingStatus.Stacks, existingStatus);
                target.Stats.AddModifier(modTemplate.Stat, statMod);
            }
            return;
        }

            // Standard overwrite: If new application is weaker, it bounces off     TODO: Should weaker spell refresh stronger one???
            if (statusEffect.Power < existingStatus.Power)
            {
                return;
            } 
            if (statusEffect.Power == existingStatus.Power && statusEffect.OverrideDuration <= existingStatus.DurationLeft)
            {
                return;
            }

            // Otherwise overwrite
            target.Stats.RemoveModifiersFromSource(existingStatus);
            target.ActiveStatuses.Remove(existingStatus);
        }

        // 3. Create the new Active Status
        var activeStatus = new ActiveStatus(template, statusEffect.OverrideDuration, statusEffect.Power, context.SourceId);
        target.ActiveStatuses.Add(activeStatus);

        // 4. Apply modifiers (Haste, Slow, Protect, etc.)
        foreach (var modTemplate in template.StatModifiers)
        {
            var statMod = new StatModifier(modTemplate.FlatValue, modTemplate.PercentValue, activeStatus);
            target.Stats.AddModifier(modTemplate.Stat, statMod);
        }

        events.Publish(new StatusAppliedEvent(context.TargetId, template.StatusName));

        //// OLD DEPRECATED SHIT ////
        /// Still need to implement "on-hit" statuses
        /// 
        /// 
        /// 
        // Example of Ledger use: Frostbolt slow shouldn't apply if damage is 0
        // TODO: Add boolean RequiresDamageToApply to ApplyStatusEffect
        // if (context.FinalDamageDealt <= 0 && statusEffect.StatusName == "Slow")
        // {
        //     return;
        // }
    }

    private void ApplyCleanse(CleanseEffect cleanse, EffectContext context, BattleActor target)
    {
        for (int i = target.ActiveStatuses.Count - 1; i >= 0; i--)
        {
            var status = target.ActiveStatuses[i];
            bool shouldCleanse = false;

            if (cleanse.CleanseAllDebuffs && !status.IsBuff) shouldCleanse = true;
            if (cleanse.StatusIdsToCleanse.Contains(status.StatusId)) shouldCleanse = true;

            if (shouldCleanse)
            {
                target.Stats.RemoveModifiersFromSource(status);
                target.ActiveStatuses.RemoveAt(i);
                events.Publish(new StatusExpiredEvent(target.Id, status.Name)); 
            }
        }
    }

    private void ApplyATBModification(ATBModifyEffect atbEffect, EffectContext context, BattleActor target)
    {
        target.ATB += atbEffect.Amount;
        if (target.ATB > target.MaxATB) target.ATB = target.MaxATB;
        if (target.ATB < 0) target.ATB = 0;
    }

    private void ApplyTeleport(TeleportToTargetEffect teleport, EffectContext context, BattleActor target)
    {
        var source = actors.GetActor(context.SourceId);
        if (source == null || source == target) return;

        // 1. Generate test points around the target (8 directions)
        int testDivision = 8; 
        double angleStep = (2 * Math.PI) / testDivision;
        float radiusOffset = target.Radius + source.Radius + 0.1f;

        List<SimVector3> testPoints = new List<SimVector3>();
        for (int i = 0; i < testDivision; i++)
        {
            double currentAngle = i * angleStep;
            float offsetX = (float)Math.Cos(currentAngle) * radiusOffset;
            float offsetZ = (float)Math.Sin(currentAngle) * radiusOffset;
            testPoints.Add(new SimVector3(target.Position.x + offsetX, target.Position.y, target.Position.z + offsetZ));
        }

        // 2. Sort by DESCENDING distance to source.
        // The point furthest from the source will be directly "behind" the target!
        testPoints.Sort((a, b) => SimVector3.Distance(source.Position, b).CompareTo(SimVector3.Distance(source.Position, a)));

        SimVector3 finalPos = source.Position;
        bool foundSpot = false;

        // 3. Find the first point that isn't occupied
        foreach (var point in testPoints)
        {
            // TODO: In the future, pass 'point' into the MapValidator to ensure we didn't generate a point floating off a cliff!
            if (!positions.IsSpaceOccupied(point, source.Radius, source.Id))
            {
                finalPos = point;
                foundSpot = true;
                break;
            }
        }

        if (!foundSpot)
        {
            events.Publish(new PlayerFeedbackEvent("Teleport Failed! No space behind target."));
            return; 
        }

        // 4. Release old space, grab new space instantly!
        // positions.ReleaseSpace(source.Id); // Clean up the space they left behind

        // TODO: Implement ReleaseSpace in PositionSystem

        source.Position = finalPos;
        positions.ReserveSpace(source.Id, finalPos);
        
        events.Publish(new ActorMovedEvent(source.Id, source.Position));
    }
}