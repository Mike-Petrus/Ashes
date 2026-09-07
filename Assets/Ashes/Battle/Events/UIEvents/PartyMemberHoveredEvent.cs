public class PartyMemberHoveredEvent : IBattleEvent
{
    public ActorId ActorId;

    public PartyMemberHoveredEvent(ActorId actorId)
    {
        ActorId = actorId;
    }
}