public class PlayerCommandEndedEvent : IBattleEvent
{
    public ActorId ActorId { get; }
    
    public PlayerCommandEndedEvent(ActorId actorId)
    {
        ActorId = actorId;
    }
}