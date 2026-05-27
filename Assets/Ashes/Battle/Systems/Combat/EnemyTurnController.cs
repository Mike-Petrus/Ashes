using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class EnemyTurnController
{
    private BattleSimulation simulation;
    private BattleCommandBuilder builder;
    private Random rng;

    public EnemyTurnController(BattleSimulation battleSimulation, BattleCommandBuilder commandBuilder)
    {
        simulation = battleSimulation;
        builder = commandBuilder;

        rng = new Random();

        simulation.Events.Subscribe<ActorReadyEvent>(OnActorReady);
    }

    private void OnActorReady(ActorReadyEvent e)
    {
        // 1. Ensure the actor exists and is an enmey
        if (!simulation.Actors.TryGetActor(e.ActorId, out var actor))
        {
            return;
        }
        if (actor.Faction != ActorFaction.Enemy)
        {
            return;
        }

        // 2. Select an ability (Default to Basic Attack for now)
        Ability selectedAbility = new BasicAttackAbility();
        float attackRange = selectedAbility.Range;
        float moveRange = actor.Stats.MoveDistance;
        float threatRange = attackRange + moveRange;

        // 3. Find a Target
        List<BattleActor> potentialTargets = new();

        foreach (var id in simulation.Actors.GetAliveActorIds())
        {
            var targetActor = simulation.Actors.GetActor(id);

            if (targetActor.Faction == ActorFaction.Party)
            {
                // Distance check to see if they are within Maximum Threat Range
                float distance = SimVector3.Distance(actor.Position, targetActor.Position);

                // Add in actor Radius
                float trueDistance = distance + actor.Radius + targetActor.Radius;

                if (trueDistance <= threatRange)
                {
                    potentialTargets.Add(targetActor);
                }
            }
        }

        // 4. Begin building the Command
        builder.BeginCommand(actor.Id);

        if (potentialTargets.Count > 0)
        {
            // Pick a random valid player
            var chosenTarget = potentialTargets[rng.Next(potentialTargets.Count)];
            TargetInfo targetInfo = TargetInfo.ForActor(chosenTarget.Id, selectedAbility.Mode);

            float currentDistance = SimVector3.Distance(actor.Position, chosenTarget.Position) - actor.Radius - chosenTarget.Radius;

            if (currentDistance <= attackRange)
            {
                // Already in range, so just attack
                builder.AddStep(new AbilityStep(actor.Id, selectedAbility, targetInfo));

                // Now wait or choose a space to move to
                builder.AddStep(new WaitStep(actor.Id));
            }
            else
            {
                // Need to move into range, then attack
                SimVector3 dirToTarget = (chosenTarget.Position - actor.Position).Normalized();

                // Calculate stopping point: stop just inside our attack range
                float distanceToMove = currentDistance - attackRange + 0.1f;
                SimVector3 moveDest = actor.Position + (dirToTarget * distanceToMove);

                // TODO: In Phase 7, we'll validate this moveDest agains the Pathfinder/PositionSystem
                // to ensure they aren't trying to stand inside a wall
                builder.AddStep(new MoveStep(actor.Id, moveDest));
                builder.AddStep(new AbilityStep(actor.Id, selectedAbility, targetInfo));
            }
        }
        else
        {
            // No targets in threat range. Move towards the closest player and Wait.
            BattleActor closestPlayer = null;
            float closestDist = float.MaxValue;

            foreach (var id in simulation.Actors.GetAliveActorIds())
            {
                var p = simulation.Actors.GetActor(id);
                if (p.Faction == ActorFaction.Party)
                {
                    float dist = SimVector3.Distance(actor.Position, p.Position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestPlayer = p;
                    }
                }
            }

            if (closestPlayer != null)
            {
                SimVector3 dirToTarget = (closestPlayer.Position - actor.Position).Normalized();
                SimVector3 moveDest = actor.Position + (dirToTarget * moveRange);
                
                builder.AddStep(new MoveStep(actor.Id, moveDest));
                builder.AddStep(new WaitStep(actor.Id));
            }
            else
            {
                // Everyone is dead? Just wait. (Enforces the 2-step pipeline rule)
                builder.AddStep(new MoveStep(actor.Id, actor.Position));
                builder.AddStep(new WaitStep(actor.Id));
            }
        }

        // 5. Submit the Command to the Queue
        simulation.ActionQueue.Enqueue(builder.Build());
    }
}