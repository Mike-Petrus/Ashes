using System;
using System.Collections.Generic;

public class TargetingSystem
{
    private ActorRegistry actors;
    private PositionSystem positions;
    private ILineOfSightChecker losChecker;

    public TargetingSystem(ActorRegistry actorRegistry, PositionSystem positionSystem, ILineOfSightChecker lineOfSightChecker)
    {
        actors = actorRegistry;
        positions = positionSystem;
        losChecker = lineOfSightChecker;
    }

    // TODO: Return valid targets when previewSystem requests them
    public void Update(float deltaTime) { }

    public List<ActorId> GetAffectedTargets(ActorId sourceId, TargetInfo targetInfo, Ability ability, TargetAlignment? overrideAlignment = null)
    {
        // We determine which alignment to filter by. 
        // For actual execution, this is ability.Alignment. 
        // For visualization previews, it is TargetAlignment.Everyone.
        TargetAlignment filterAlignment = overrideAlignment ?? ability.Alignment;

        switch (targetInfo.Mode)
        {
            case TargetingMode.Self:
                return GetSelfTargets(sourceId, ability, filterAlignment);

            case TargetingMode.SingleTarget:
                return GetSingleTarget(targetInfo, ability);

            case TargetingMode.PointAoE:
                return GetPointAoETargets(sourceId, targetInfo, ability, filterAlignment);

            case TargetingMode.ActorAoE:
                return GetActorAoETargets(sourceId, targetInfo, ability, filterAlignment);

            case TargetingMode.HybridAoE:
                return GetHybridAoETargets(sourceId, targetInfo, ability, filterAlignment);

            case TargetingMode.Directional:
                return GetDirectionalTargets(sourceId, targetInfo, ability, filterAlignment);

            default:
                return new List<ActorId>();
        }
    }

    private List<ActorId> GetSelfTargets(ActorId sourceId, Ability ability, TargetAlignment filterAlignment)
    {
        if (ability.Radius > 0)
        {
            var sourceActor = actors.GetActor(sourceId);
            return FilterByAlignment(sourceId, positions.GetActorsInRadius(sourceActor.Position, ability.Radius), filterAlignment);
        }

        return new List<ActorId> { sourceId };
    }

    private List<ActorId> GetSingleTarget(TargetInfo targetInfo, Ability ability)
    {
        if (targetInfo.TargetActor.HasValue)
        {
            // TODO: Does this need alignmentcheck???
            return new List<ActorId> { targetInfo.TargetActor.Value };
        }

        return new List<ActorId>() ;
    }

    private List<ActorId> GetPointAoETargets(ActorId sourceId, TargetInfo targetInfo, Ability ability, TargetAlignment filterAlignment)
    {
        var caughtActors = positions.GetActorsInRadius(targetInfo.TargetPosition, ability.Radius);

        if (ability.RequiresLoS)
        {
            caughtActors.RemoveAll(id =>
            {
                var target = actors.GetActor(id);
                return !losChecker.HasLineOfSight(targetInfo.TargetPosition, target.Position);
            });
        }

        return FilterByAlignment(sourceId, caughtActors, filterAlignment);
    }

    private List<ActorId> GetActorAoETargets(ActorId sourceId, TargetInfo targetInfo, Ability ability, TargetAlignment filterAlignment)
    {
        if (!targetInfo.TargetActor.HasValue)
        {
            return new List<ActorId>();
        }

        var mainTarget = actors.GetActor(targetInfo.TargetActor.Value);

        if (mainTarget == null)
        {
            return new List<ActorId>();
        }

        return FilterByAlignment(sourceId, positions.GetActorsInRadius(mainTarget.Position, ability.Radius), filterAlignment);
    }

    private List<ActorId> GetHybridAoETargets(ActorId sourceId, TargetInfo targetInfo, Ability ability, TargetAlignment filterAlignment)
    {
        if (targetInfo.TargetActor.HasValue)
        {
            return GetActorAoETargets(sourceId, targetInfo, ability, filterAlignment);
        }
        else
        {
            return GetPointAoETargets(sourceId, targetInfo, ability, filterAlignment);
        }
    }

    private List<ActorId> GetDirectionalTargets(ActorId sourceId, TargetInfo targetInfo, Ability ability, TargetAlignment filterAlignment)
    {
        // TODO: The math for Line/Cone attacks. 
        // 1. Get Source position.
        // 2. Get Target position (either from targetInfo.TargetPosition OR targetActor.Position)
        // 3. Calculate forward vector.
        // 4. Do Dot-Product checks against all actors in Range.

        List<ActorId> hitTargets = new();
        var sourceActor = actors.GetActor(sourceId);

        // 1. Determine source vector
        SimVector3 targetPos = targetInfo.TargetActor.HasValue ? actors.GetActor(targetInfo.TargetActor.Value).Position : targetInfo.TargetPosition;
        SimVector3 forwardDir = (targetPos - sourceActor.Position).Normalized();

        double angleThreshold = Math.Cos((ability.Angle / 2f) * (Math.PI / 180f));

        // 2. Evaluate all actors
        foreach (var actor in actors.GetAllActors())
        {
            if (actor.Id == sourceId)
            {
                continue;
            }

            float distance = SimVector3.Distance(actor.Position, sourceActor.Position);

            if (distance - actor.Radius <= ability.Radius)
            {
                SimVector3 normalizedDir = (actor.Position - sourceActor.Position).Normalized();
                double dotProduct = SimVector3.DotProduct(forwardDir, normalizedDir);

                if (dotProduct >= angleThreshold)
                {
                    if (!ability.RequiresLoS || losChecker.HasLineOfSight(sourceActor.Position, actor.Position))
                    {
                        hitTargets.Add(actor.Id);
                    }
                }
            }
        }

        return FilterByAlignment(sourceId, hitTargets, filterAlignment);
    }

    public List<ActorId> FilterByAlignment(ActorId sourceId, List<ActorId> actorIds, TargetAlignment alignment, bool canTargetDead = false)
    {
        List<ActorId> filteredList = new();
        var sourceActor = actors.GetActor(sourceId);

        foreach (var id in actorIds)
        {
            var targetActor = actors.GetActor(id);

            if (!targetActor.IsAlive && !canTargetDead)
            {
                continue;
            }
            
            bool isAlly = sourceActor.Faction == targetActor.Faction;
            bool isValid = false;

            switch (alignment)
            {
                case TargetAlignment.Everyone:
                    isValid = true;
                    break;

                case TargetAlignment.SelfOnly:
                    isValid = id.Equals(sourceId);
                    break;

                case TargetAlignment.Ally:
                    isValid = isAlly;
                    break;

                case TargetAlignment.Enemy:
                    isValid = !isAlly;
                    break;
            }

            if (isValid)
            {
                filteredList.Add(id);
            }
        }

        return filteredList;
    }
}