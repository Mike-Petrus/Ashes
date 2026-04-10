public class ActorReadyEvent : IBattleEvent
{
    public ActorId ActorId;

    public ActorReadyEvent(ActorId actorId)
    {
        ActorId = actorId;
    }
}