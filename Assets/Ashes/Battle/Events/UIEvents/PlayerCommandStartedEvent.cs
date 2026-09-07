public class PlayerCommandStartedEvent : IBattleEvent
{
    public ActorId ActorId { get; }

    public PlayerCommandStartedEvent(ActorId actorId)
    {
        ActorId = actorId;
    }
}