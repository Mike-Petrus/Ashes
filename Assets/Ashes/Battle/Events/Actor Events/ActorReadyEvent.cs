public struct ActorReadyEvent
{
    public ActorId ActorId;

    public ActorReadyEvent(ActorId actorId)
    {
        ActorId = actorId;
    }
}