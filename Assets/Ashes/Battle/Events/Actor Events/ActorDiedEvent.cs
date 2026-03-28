public class ActorDiedEvent : IBattleEvent
{
    public ActorId ActorId;

    public ActorDiedEvent(ActorId actorId)
    {
        ActorId = actorId;
    }
}