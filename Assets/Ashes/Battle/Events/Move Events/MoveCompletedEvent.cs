public struct MoveCompletedEvent
{
    public ActorId ActorId;

    public MoveCompletedEvent(ActorId actorId)
    {
        ActorId = actorId;
    }
}