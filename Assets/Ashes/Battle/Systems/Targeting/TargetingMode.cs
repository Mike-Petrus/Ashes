public enum TargetingMode
{
    Self,
    SingleTarget,       // Must select an Actor. Relies on ActorId as target
    PointAoE,           // Must select a coordinate. Relies on SimVector3 (Grenades, Rain)
    ActorAoE,           // Must select an Actor. AoE tracks Actor target (Aura, Living Bomb)
    HybridAoE,          // UI allows you to select position or target. Execution checks TargetActor.hasValue (Covers most abilities)
    Directional         // Cone/Line attacks - Assumes hybrid selection
}