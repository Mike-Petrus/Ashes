public struct TargetInfo
{
    public TargetingMode Mode;
    public ActorId? TargetActor;
    public SimVector3 TargetPosition;

    // Used for: SingleTarget, ActorAoE, Directional (locked on)
    // Factory methods build TargetInfo struct. Defaults to SingleTarget
    public static TargetInfo ForActor(ActorId targetId, TargetingMode mode = TargetingMode.SingleTarget) =>
        new TargetInfo { Mode = mode, TargetActor = targetId };

    // Used for: PointAoE, Directional (Aimed at position)
    // Defaults PointAoE
    public static TargetInfo ForPosition(SimVector3 position, TargetingMode mode = TargetingMode.PointAoE) =>
        new TargetInfo { Mode = mode, TargetPosition = position };

    public static TargetInfo ForSelf(ActorId selfId) =>
        new TargetInfo { Mode = TargetingMode.Self, TargetActor = selfId };
}