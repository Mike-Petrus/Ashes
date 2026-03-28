public class MoveCompletedEvent : IBattleEvent
{
    public ActorId ActorId;

    public MoveCompletedEvent(ActorId actorId)
    {
        ActorId = actorId;
    }
}