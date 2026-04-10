public class ATBChangeRequestEvent : IBattleEvent
{
    public ActorId ActorId;
    public float RequestPercent;
    public bool IsNegative;

    public ATBChangeRequestEvent(ActorId actorId, float requestPercent, bool isNegative)
    {
        ActorId = actorId;
        RequestPercent = requestPercent;
        IsNegative = isNegative;
    }
}