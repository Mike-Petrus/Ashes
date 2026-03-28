public class DamageRequestEvent : IBattleEvent
{
    public ActorId SourceId;
    public ActorId TargetId;
    public float Amount;

    public DamageRequestEvent(ActorId sourceId, ActorId targetId, float amount)
    {
        SourceId = sourceId;
        TargetId = targetId;
        Amount = amount;
    }
}