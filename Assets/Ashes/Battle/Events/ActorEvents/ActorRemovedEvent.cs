public class ActorRemovedEvent : IBattleEvent
{
    public ActorId ActorId { get; }

    public ActorRemovedEvent(ActorId actorId)
    {
        ActorId = actorId;
    }
}