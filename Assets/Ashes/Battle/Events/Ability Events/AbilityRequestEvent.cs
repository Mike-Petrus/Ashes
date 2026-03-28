public class AbilityRequestEvent : IBattleEvent
{
    public ActorId ActorId;
    public Ability Ability;
    public ActorId TargetId;

    public AbilityRequestEvent(ActorId actorId, Ability ability, ActorId targetId)
    {
        ActorId = actorId;
        Ability = ability;
        TargetId = targetId;
    }
}