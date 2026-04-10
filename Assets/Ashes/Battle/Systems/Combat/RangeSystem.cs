public class RangeSystem
{
    private readonly ActorRegistry actors;

    public RangeSystem(ActorRegistry actorRegistry)
    {
        actors = actorRegistry;
    }

    public void Update(float deltaTime) { }

    // Used by Example 2 (Static Move then Attack)
    // Checks if a target is in range from a SPECIFIC starting coordinate
    public bool IsInRange(SimVector3 sourcePosition, float sourceRadius, Ability ability, TargetInfo targetInfo)
    {
        if (targetInfo.Mode == TargetingMode.Self) return true;

        SimVector3 targetPos;
        float targetRadius = 0f;

        // Where is the target?
        if (targetInfo.TargetActor.HasValue)
        {
            var targetActor = actors.GetActor(targetInfo.TargetActor.Value);
            if (targetActor == null || !targetActor.IsAlive) return false;
            
            targetPos = targetActor.Position;
            targetRadius = targetActor.Radius;
        }
        else
        {
            targetPos = targetInfo.TargetPosition;
        }

        // The Math: Distance between centers must be <= (Weapon Range + Both Hitbox Radii)
        float distance = SimVector3.Distance(sourcePosition, targetPos);
        return distance <= (ability.Range + sourceRadius + targetRadius);
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