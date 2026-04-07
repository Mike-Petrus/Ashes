using System;
using System.Collections.Generic;

public class EffectPipeline : IBattleSystem
{
    private BattleEventBus events;
    private ActorRegistry actors;

    public EffectPipeline(BattleEventBus eventBus, ActorRegistry actorRegistry)
    {
        events = eventBus;
        actors = actorRegistry;

        events.Subscribe<EffectRequestEvent>(OnEffectRequest);
        events.Subscribe<EffectTickRequestEvent>(OnEffectTickRequest);
    }

    public void Update(float deltaTime)
    {
        
    }

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

        // 1. Validation check
        if (target == null || !target.IsAlive)
        {
            return;
        }

        // 2. TODO: Hit check (evasion/accuracy)
        // e.Context.IsHit = CalculateHitChance(e.Context.SourceId, target);
        // if (!e.Context.IsHit) { events.Publish(new EvadedEvent())... return; }

        // 3. Process every effect sequentially
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
                ApplyStatus(status, context, target);
            }
        }
    }

    private void ApplyDamage(DamageEffect damageEffect, EffectContext context, BattleActor target)
    {
        // 1. The RPG Math (Armor, Elemental weakness, Defend State)
        // e.g. int mitigatedDamage = damageEffect.BaseDamage - target.Stats.Armor;
        int finalDamage = Math.Max(0, damageEffect.BaseDamage);         // ensure we dont heal  the target

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
        // Example of Ledger use: Frostbolt slow shouldn't apply if damage is 0
        // TODO: Add boolean RequiresDamageToApply to ApplyStatusEffect
        if (context.FinalDamageDealt <= 0 && statusEffect.StatusName == "Slow")
        {
            return;
        }

        var activeStatus = new ActiveStatus(statusEffect, context.SourceId);
        target.ActiveStatuses.Add(activeStatus);

        events.Publish(new StatusAppliedEvent(context.TargetId, statusEffect.StatusName));
        // TODO: In the future, we will hand this to the StatusEffectSystem 
        // target.ActiveEffects.Add(new ActiveStatus(statusEffect.StatusName, ...));
        
        // For now, we can just fire an event or log it
        // _events.Publish(new StatusAppliedEvent(context.SourceId, context.TargetId, statusEffect.StatusName));
    }
}