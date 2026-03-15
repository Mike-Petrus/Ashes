public struct AbilityCompletedEvent
{
    public ActorId ActorId;
    public Ability Ability;
    public ActorId TargetId;

    public AbilityCompletedEvent(ActorId actorId, Ability ability, ActorId targetId)
    {
        ActorId = actorId;
        Ability = ability;
        TargetId = targetId;
    }
}