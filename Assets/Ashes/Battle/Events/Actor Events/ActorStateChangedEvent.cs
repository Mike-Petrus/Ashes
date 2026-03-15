public struct ActorStateChangedEvent
{
    public ActorId ActorId;
    public ActorState State;

    public ActorStateChangedEvent(ActorId actorId, ActorState state)
    {
        ActorId = actorId;
        State = state;
    }
}