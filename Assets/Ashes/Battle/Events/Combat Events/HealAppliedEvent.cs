public class HealAppliedEvent : IBattleEvent
{
    public ActorId SourceId;
    public ActorId TargetId;
    public int Amount;

    public HealAppliedEvent(ActorId sourceId, ActorId targetId, int amount)
    {
        SourceId = sourceId;
        TargetId = targetId;
        Amount = amount;
    }
}