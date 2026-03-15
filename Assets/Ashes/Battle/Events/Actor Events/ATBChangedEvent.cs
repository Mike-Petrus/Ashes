public struct ATBChangedEvent
{
    public ActorId ActorId;
    public float ActorATB;

    public ATBChangedEvent(ActorId actorId, float actorATB)
    {
        ActorId = actorId;
        ActorATB = actorATB;
    }
}