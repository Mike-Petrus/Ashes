public class EffectContext
{
    public ActorId SourceId { get; }
    public ActorId TargetId { get; }

    // Ledger - modified by pipeline
    public bool IsHit { get; set; } = true;
    public bool IsCritical { get; set; } = false;
    public int FinalDamageDealt { get; set; } = 0;

    public EffectContext(ActorId sourceId, ActorId targetId)
    {
        SourceId = sourceId;
        TargetId = targetId;
    }
}