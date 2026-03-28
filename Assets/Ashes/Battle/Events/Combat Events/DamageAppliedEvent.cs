public class DamageAppliedEvent : IBattleEvent
{
    public ActorId SourceId;
    public ActorId TargetId;
    public float Amount;

    public DamageAppliedEvent(ActorId sourceId, ActorId targetId, float amount)
    {
        SourceId = sourceId;
        TargetId = targetId;
        Amount = amount;
    }
}