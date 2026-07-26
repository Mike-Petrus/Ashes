using System;
using System.Collections.Generic;

public class BasicAIProcessor : IAIProcessor
{
    public BattleCommand DetermineAction(ActorId actorId, BattleSimulation simulation, BattleCommandBuilder builder)
    {
        builder.Clear();
        builder.BeginCommand(actorId);

        var actor = simulation.Actors.GetActor(actorId);

        // 1. Find a target
        ActorId? bestTarget = null;
        SimVector3? plannedMoveDestination = null;

        Ability defaultAbility = GetDefaultAbility(actor.Abilities);

        float closestDist = float.MaxValue;
        int lowestHP = int.MaxValue;
        bool canAttackWithoutMoving = false;

        foreach (var target in simulation.Actors.GetAliveActorsByFaction(ActorFaction.Party))
        {
            var targetInfo = TargetInfo.ForActor(target.Id, defaultAbility.Mode);
            float distance = SimVector3.Distance(actor.Position, target.Position);

            // Target Priority
            // 1. Can attack target without moving?
            if (simulation.RangeSystem.IsInRange(actor.Position, actor.Radius, defaultAbility, targetInfo))
            {
                if (!canAttackWithoutMoving || target.Stats.CurrentHP < lowestHP)
                {
                    bestTarget = target.Id;
                    lowestHP = target.Stats.CurrentHP;
                    closestDist = distance;
                    plannedMoveDestination = null;
                    canAttackWithoutMoving = true;
                }
            }
            // 2. If can't attack without moving, is there a path to this target?
            else if (!canAttackWithoutMoving)
            {
                if (TacticalPathfindingUtility.TryCalculateMoveDestination(actor, target, defaultAbility, simulation.BattleContext, out SimVector3 validDest))
                {
                    if (target.Stats.CurrentHP < lowestHP)
                    {
                        bestTarget = target.Id;
                        lowestHP = target.Stats.CurrentHP;
                        closestDist = distance;
                        plannedMoveDestination = validDest;
                    }
                }
                // 3. If can't reach any target, is this the closest target? 
                // TODO: Not sure about this logic but entire AI stack will require in depth rewrite
                else if (bestTarget == null || (plannedMoveDestination == null && distance < closestDist))
                {
                    bestTarget = target.Id;
                    closestDist = distance;

                    // Path towards closest perimiter point
                    // TODO: May want to recalculate a fallback point instead of using target.Position which is blocked
                    List<SimVector3> testPoints = TacticalPathfindingUtility.GetTestPoints(actor, target);
                    SimVector3 closestMoveTarget = testPoints.Count > 0 ? testPoints[0] : target.Position;

                    plannedMoveDestination = TacticalPathfindingUtility.CalculatePartialMove(actor, closestMoveTarget, simulation.BattleContext);
                }
            }
        }

        if (plannedMoveDestination.HasValue && SimVector3.Distance(actor.Position, plannedMoveDestination.Value) > 0.1f)
        {
            builder.AddStep(new MoveStep(actorId, plannedMoveDestination.Value));
        }


        if (bestTarget.HasValue)
        {
            var targetInfo = TargetInfo.ForActor(bestTarget.Value, defaultAbility.Mode);

            // Verify in range from final position
            SimVector3 finalPos = plannedMoveDestination ?? actor.Position;

            if (simulation.RangeSystem.IsInRange(finalPos, actor.Radius, defaultAbility, targetInfo))
            {
                builder.AddStep(new AbilityStep(actorId, defaultAbility, targetInfo));
            }
        }

        // ENFORCE COMMAND RULES: WaitStep cannot be the first or only step
        // If the AI is completely trapped/out of range, generate a 0-distance MoveStep
        // TODO: Allow AI to skip turn for edge cases
        if (builder.Size == 0)
        {
            builder.AddStep(new MoveStep(actorId, actor.Position));
        }
        if (builder.Size < 2)
        {
            builder.AddStep(new WaitStep(actorId));
        }

        return builder.Build();
    }

    private Ability GetDefaultAbility(AbilitySet abilitySet)
    {
        // Search dictionary and get first available ability, or just use basic attack
        foreach (var category in abilitySet.AvailableAbilities.Values)
        {
            if (category != null && category.Count > 0)
            {
                return category[0];
            }
        }

        return new BasicAttackAbility();
    }    
}