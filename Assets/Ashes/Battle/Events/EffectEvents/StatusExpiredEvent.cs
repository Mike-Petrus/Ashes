public class StatusExpiredEvent : IBattleEvent
{
    public ActorId TargetId;
    public string StatusName;

    public StatusExpiredEvent(ActorId targetId, string statusName)
    {
        TargetId = targetId;
        StatusName = statusName;
    }
}