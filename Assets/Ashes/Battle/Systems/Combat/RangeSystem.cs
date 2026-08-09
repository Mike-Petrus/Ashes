using System;

public class RangeSystem
{
    private ActorRegistry actors;
    private ILineOfSightChecker losChecker;

    public RangeSystem(ActorRegistry actorRegistry, ILineOfSightChecker lineOfSightChecker = null)
    {
        actors = actorRegistry;
        losChecker = lineOfSightChecker;
    }

    public void Update(float deltaTime) { }

    public bool IsInRange(SimVector3 sourcePosition, float sourceRadius, Ability ability, TargetInfo targetInfo)
    {
        return IsInRange(sourcePosition, sourceRadius, ability, targetInfo, out _);
    }

    // Used by Example 2 (Static Move then Attack)
    // Checks if a target is in range from a SPECIFIC starting coordinate
    public bool IsInRange(SimVector3 sourcePosition, float sourceRadius, Ability ability, TargetInfo targetInfo, out string errorMessage)
    {
        errorMessage = "";

        if (targetInfo.Mode == TargetingMode.Self) return true;

        SimVector3 targetPos = targetInfo.TargetPosition;;
        float targetRadius = 0f;

        // Where is the target?
        if (targetInfo.TargetActor.HasValue)
        {
            var targetActor = actors.GetActor(targetInfo.TargetActor.Value);
            if (targetActor == null || !targetActor.IsAlive)
            {
                errorMessage = "Invalid Target!";
                return false;
            }
            
            targetPos = targetActor.Position;
            targetRadius = targetActor.Radius;
        }
        else
        {
            targetPos = targetInfo.TargetPosition;
        }

        // 1. Distance Check
        float distance = SimVector3.Distance(sourcePosition, targetPos);
        float effectiveDistance = distance;

        if (targetInfo.TargetActor.HasValue)
        {
            var targetActor = actors.GetActor(targetInfo.TargetActor.Value);
            effectiveDistance = Math.Max(0, distance - (sourceRadius + targetActor.Radius));
        }
        else
        {
            effectiveDistance = Math.Max(0, distance - sourceRadius);
        }

        if (effectiveDistance > ability.Range)
        {
            errorMessage = "Out of Range!";
            return false;
        }

        // 2. Line of Sight Check
        if (ability.RequiresLoS && losChecker != null)
        {
            if (!losChecker.HasLineOfSight(sourcePosition, targetPos))
            {
                errorMessage = "Sight Obstructed!";
                return false;
            }
        }

        return true;
    }

    // Overload used by Example 1 (Execution phase checks)
    // Checks if a target is in range from the Actor's CURRENT position
    public bool IsActorInRange(ActorId sourceId, Ability ability, TargetInfo targetInfo)
    {
        var sourceActor = actors.GetActor(sourceId);
        if (sourceActor == null) return false;

        return IsInRange(sourceActor.Position, sourceActor.Radius, ability, targetInfo);
    }
}