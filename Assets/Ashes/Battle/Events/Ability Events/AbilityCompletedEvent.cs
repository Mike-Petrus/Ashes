public class AbilityCompletedEvent : IBattleEvent
{
    public ActorId ActorId;
    public Ability Ability;
    public TargetInfo TargetInfo;

    public AbilityCompletedEvent(ActorId actorId, Ability ability, TargetInfo targetInfo)
    {
        ActorId = actorId;
        Ability = ability;
        TargetInfo = targetInfo;
    }
}