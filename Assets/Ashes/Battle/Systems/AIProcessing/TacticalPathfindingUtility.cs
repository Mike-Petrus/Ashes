using System;
using System.Collections.Generic;

public static class TacticalPathfindingUtility
{
    public static bool TryCalculateMoveDestination(BattleActor actor, BattleActor target, Ability ability, BattleContext context, out SimVector3 destination)
    {
        destination = actor.Position;
        float maxMoveDist = actor.Stats.MoveDistance;

        List<SimVector3> testPoints = GetTestPoints(actor, target);

        foreach (var point in testPoints)
        {
            // 1. Is the final position occupied?
            if (context.Position.IsSpaceOccupied(point, actor.Radius, actor.Id))
            {
                continue;
            }

            // 2. Can we find a valid path?
            var path = context.Path.FindPath(actor.Position, point, actor.Radius);
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

                currentPos = node;
                currentMoveDist += stepDist;
            }

            if (validPath)
            {
                // Safely handle "Dummy Follow" abilities that might not have a Mode setup
                var targetInfo = TargetInfo.ForActor(target.Id, ability?.Mode ?? TargetingMode.SingleTarget);

                if (ability == null || context.Range.IsInRange(currentPos, actor.Radius, ability, targetInfo))
                {
                    destination = currentPos;
                    return true;
                }
            }
        }

        return false;
    }

    public static SimVector3 CalculatePartialMove(BattleActor actor, SimVector3 targetPos, BattleContext context)
    {
        var path = context.Path.FindPath(actor.Position, targetPos, actor.Radius);
        SimVector3 destination = actor.Position;

        if (path == null || path.Count == 0) return destination;

        float currentMoveDist = 0f;
        float maxMoveDist = actor.Stats.MoveDistance;

        List<SimVector3> traversedNodes = new List<SimVector3> { destination };

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
            if (!context.Position.IsSpaceOccupied(traversedNodes[i], actor.Radius, actor.Id))
            {
                return traversedNodes[i];
            }
        }

        // Fallback. If there's nowhere to move, stay put
        return actor.Position;
    }

    public static List<SimVector3> GetTestPoints(BattleActor actor, BattleActor target)
    {
        List<SimVector3> validPoints = new();

        int testDivision = 16;
        double angleStep = (2 * Math.PI) / testDivision;

        float radiusOffset = actor.Radius + target.Radius + 0.1f;

        for (int i = 0; i < testDivision; i++)
        {
            double currentAngle = i * angleStep;

            float offsetX = (float)Math.Cos(currentAngle) * radiusOffset;
            float offsetZ = (float)Math.Sin(currentAngle) * radiusOffset;

            SimVector3 destinationPoint = new SimVector3(target.Position.x + offsetX, target.Position.y, target.Position.z + offsetZ);
            validPoints.Add(destinationPoint);
        }

        // Sort by distance to prioritize front-facing approach
        validPoints.Sort((a, b) => SimVector3.Distance(actor.Position, a).CompareTo(SimVector3.Distance(actor.Position, b)));

        return validPoints;
    }
}