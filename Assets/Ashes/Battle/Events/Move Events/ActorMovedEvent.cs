public struct ActorMovedEvent
{
    public ActorId ActorId;
    public SimVector3 Position;

    public ActorMovedEvent(ActorId actorId, SimVector3 position)
    {
        ActorId = actorId;
        Position = position;
    }
}