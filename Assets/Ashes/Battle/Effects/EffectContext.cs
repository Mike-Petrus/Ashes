public class EffectContext
{
    public ActorId SourceId { get; }
    public ActorId TargetId { get; }
    public string AbilityId { get; }

    // Ledger - modified by pipeline
    public bool IsHit { get; set; } = true;
    public bool IsCritical { get; set; } = false;
    public int FinalDamageDealt { get; set; } = 0;

    public EffectContext(ActorId sourceId, ActorId targetId, string abilityId = null)
    {
        SourceId = sourceId;
        TargetId = targetId;
        AbilityId = abilityId;
    }
}