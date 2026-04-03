using System.Collections.Generic;

public class TargetingSystem
{
    private ActorRegistry actors;
    private PositionSystem positions;

    public TargetingSystem(ActorRegistry actorRegistry, PositionSystem positionSystem)
    {
        actors = actorRegistry;
        positions = positionSystem;
    }

    // TODO: Return valid targets when previewSystem requests them
    public void Update(float deltaTime) { }

    public List<ActorId> GetAffectedTargets(ActorId sourceId, TargetInfo targetInfo, Ability ability)
    {
        switch (targetInfo.Mode)
        {
            case TargetingMode.Self:
                return GetSelfTargets(sourceId, ability);

            case TargetingMode.SingleTarget:
                return GetSingleTarget(targetInfo, ability);

            case TargetingMode.PointAoE:
                return GetPointAoETargets(sourceId, targetInfo, ability);

            case TargetingMode.ActorAoE:
                return GetActorAoETargets(sourceId, targetInfo, ability);

            case TargetingMode.HybridAoE:
                return GetHybridAoETargets(sourceId, targetInfo, ability);

            case TargetingMode.Directional:
                return GetDirectionalTargets(sourceId, targetInfo, ability);

            default:
                return new List<ActorId>();
        }
    }

    private List<ActorId> GetSelfTargets(ActorId sourceId, Ability ability)
    {
        if (ability.Radius > 0)
        {
            var sourceActor = actors.GetActor(sourceId);
            return FilterByAlignment(sourceId, positions.GetActorsInRadius(sourceActor.Position, ability.Radius), ability.Alignment);
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

    private List<ActorId> GetPointAoETargets(ActorId sourceId, TargetInfo targetInfo, Ability ability)
    {
        // TODO: This DOES need alignment check. Probably fine doing it this way
        return FilterByAlignment(sourceId, positions.GetActorsInRadius(targetInfo.TargetPosition, ability.Radius), ability.Alignment);
    }

    private List<ActorId> GetActorAoETargets(ActorId sourceId, TargetInfo targetInfo, Ability ability)
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

        return FilterByAlignment(sourceId, positions.GetActorsInRadius(mainTarget.Position, ability.Radius), ability.Alignment);
    }

    private List<ActorId> GetHybridAoETargets(ActorId sourceId, TargetInfo targetInfo, Ability ability)
    {
        if (targetInfo.TargetActor.HasValue)
        {
            return GetActorAoETargets(sourceId, targetInfo, ability);
        }
        else
        {
            return GetPointAoETargets(sourceId, targetInfo, ability);
        }
    }

    private List<ActorId> GetDirectionalTargets(ActorId sourceId, TargetInfo targetInfo, Ability ability)
    {
        // TODO: The math for Line/Cone attacks. 
        // 1. Get Source position.
        // 2. Get Target position (either from targetInfo.TargetPosition OR targetActor.Position)
        // 3. Calculate forward vector.
        // 4. Do Dot-Product checks against all actors in Range.
        return new List<ActorId>(); 
    }

    private List<ActorId> FilterByAlignment(ActorId sourceId, List<ActorId> actorIds, TargetAlignment alignment)
    {
        // TODO: Alignment logic - check if actor is in player party or enemy 
        return new List<ActorId>();
    }
}