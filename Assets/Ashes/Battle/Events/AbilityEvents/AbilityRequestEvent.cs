public class AbilityRequestEvent : IBattleEvent
{
    public ActorId ActorId;
    public Ability Ability;
    public TargetInfo TargetInfo;

    public AbilityRequestEvent(ActorId actorId, Ability ability, TargetInfo targetInfo)
    {
        ActorId = actorId;
        Ability = ability;
        TargetInfo = targetInfo;
    }
}