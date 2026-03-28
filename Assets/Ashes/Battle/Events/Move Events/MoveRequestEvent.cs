public class MoveRequestEvent : IBattleEvent
{
    public ActorId ActorId;
    public SimVector3 Start;
    public SimVector3 Destination;

    public MoveRequestEvent(ActorId actorId, SimVector3 start, SimVector3 destination)
    {
        ActorId = actorId;
        Start = start;
        Destination = destination;
    }
}