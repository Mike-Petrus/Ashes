public class StatusAppliedEvent : IBattleEvent
{
    public ActorId TargetId;
    public string StatusName;

    public StatusAppliedEvent(ActorId targetId, string statusName)
    {
        TargetId = targetId;
        StatusName = statusName;
    }
}