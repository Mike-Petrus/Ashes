public class ATBRequestCompletedEvent : IBattleEvent
{
    public ActorId ActorId;
    public float RefundPercent;
    public bool IsNegative;

    public ATBRequestCompletedEvent(ActorId actorId, float refundPercent, bool isNegative)
    {
        ActorId = actorId;
        RefundPercent = refundPercent;
        IsNegative = isNegative;
    }
}