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
                if (TryCalculateMoveDestination(actor, target, defaultAbility, simulation, out SimVector3 validDest))
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
                    List<SimVector3> testPoints = GetTestPoints(actor, target);
                    SimVector3 closestMoveTarget = testPoints.Count > 0 ? testPoints[0] : target.Position;

                    plannedMoveDestination = CalculatePartialMove(actor, closestMoveTarget, simulation);
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

    private bool TryCalculateMoveDestination(BattleActor actor, BattleActor target, Ability ability, BattleSimulation simulation, out SimVector3 destination)
    {
        destination = actor.Position;
        float maxMoveDist = actor.Stats.MoveDistance;

        List<SimVector3> testPoints = GetTestPoints(actor, target);

        foreach (var point in testPoints)
        {
            // 1. Is the final position occupied?
            if (simulation.PositionSystem.IsSpaceOccupied(point, actor.Radius, actor.Id))
            {
                continue;
            }

            // 2. Can we find a valid path?
            var path = simulation.Pathfinder.FindPath(actor.Position, point, actor.Radius);
            if (path == null || path.Count == 0)
            {
                continue;
            }

            // 3. Is the path within the Actor's move range?
            float currentMoveDist = 0f;
            SimVector3 currentPos = actor.Position;
            bool validPath = true;

            foreach (var node in path)
            {
                float stepDist = SimVector3.Distance(currentPos, node);

                if (currentMoveDist + stepDist > maxMoveDist)
                {
                    validPath = false;
                    break;
                }

                // The PathFinder should already avoid obstacles
                // If a space is reserved it may be considered "occupied" even though nothing is there
                // we don't care if a path crosses through a reserved space if it is empty during execution
                // but if there are issues we can add this check back in

                // if (simulation.PositionSystem.IsSpaceOccupied(node, actor.Radius, actor.Id))
                // {
                //     validPath = false;
                //     break;
                // }

                currentPos = node;
                currentMoveDist += stepDist;
            }

            if (validPath)
            {
                var targetInfo = TargetInfo.ForActor(target.Id, ability.Mode);

                if (simulation.RangeSystem.IsInRange(currentPos, actor.Radius, ability, targetInfo))
                {
                    destination = currentPos;
                    return true;
                }
            }
        }

        return false;
    }

    private SimVector3 CalculatePartialMove(BattleActor actor, SimVector3 targetPos, BattleSimulation simulation)
    {
        var path = simulation.Pathfinder.FindPath(actor.Position, targetPos, actor.Radius);
        
        // Fallback if pathfinder completely fails
        if (path == null || path.Count == 0)
        {
            return actor.Position;
        }

        SimVector3 destination = actor.Position;
        List<SimVector3> traversedNodes = new List<SimVector3> { destination };  // keep track of each step

        float currentMoveDist = 0f;
        float maxMoveDist = actor.Stats.MoveDistance;

        // 1. Walk path as far as possible, ignoring intermediate obstacles
        foreach (var node in path)
        {
            float stepDist = SimVector3.Distance(destination, node);

            if (currentMoveDist + stepDist > maxMoveDist)
            {
                // NavMesh generates corners, not a series of short steps
                // In a straight line, if the end point is too far, we must
                // generate an intermediate point to use up move budget
                float remainingDist = maxMoveDist - currentMoveDist;
                SimVector3 direction = (node - destination).Normalized();
                SimVector3 interpolatedDest = destination + (direction * remainingDist);

                traversedNodes.Add(interpolatedDest);
                break;
            }

            destination = node;
            currentMoveDist += stepDist;
            traversedNodes.Add(destination);
        }

        // 2. Step backwards from furthest reached node until we find a space that isn't occupied
        for (int i = traversedNodes.Count - 1; i >= 0; i--)
        {
            if (!simulation.PositionSystem.IsSpaceOccupied(traversedNodes[i], actor.Radius, actor.Id))
            {
                return traversedNodes[i];
            }
        }

        // Fallback. If there's nowhere to move, stay put
        return actor.Position;
    }

    private List<SimVector3> GetTestPoints(BattleActor actor, BattleActor target)
    {
        List<SimVector3> validPoints = new();

        int testDivision = 16;
        double angleStep = (2 * Math.PI) / testDivision;

        float radiusOffset = actor.Radius + target.Radius + 0.1f;

        // Search in a circle around target perimeter
        for (int i = 0; i < testDivision; i++)
        {
            double currentAngle = i * angleStep;

            float offsetX = (float)Math.Cos(currentAngle) * radiusOffset;
            float offsetZ = (float)Math.Sin(currentAngle) * radiusOffset;

            SimVector3 destinationPoint = new SimVector3(target.Position.x + offsetX, target.Position.y, target.Position.z + offsetZ);

            validPoints.Add(destinationPoint);
        }

        // Sort points by distance to prioritize points on front vs back
        validPoints.Sort((a, b) => SimVector3.Distance(actor.Position, a).CompareTo(SimVector3.Distance(actor.Position,b)));

        return validPoints;
    }
}