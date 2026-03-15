public struct ActorDiedEvent
{
    public ActorId ActorId;

    public ActorDiedEvent(ActorId actorId)
    {
        ActorId = actorId;
    }
}