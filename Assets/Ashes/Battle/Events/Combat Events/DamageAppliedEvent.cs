public class DamageAppliedEvent : IBattleEvent
{
    public ActorId SourceId;
    public ActorId TargetId;
    public int Amount;

    public DamageAppliedEvent(ActorId sourceId, ActorId targetId, int amount)
    {
        SourceId = sourceId;
        TargetId = targetId;
        Amount = amount;
    }
}