public class AbilityContext
{
    public ActorId SourceId;
    public TargetInfo TargetInfo;

    public BattleEventBus Events;
    public TargetingSystem Targeting;

    public AbilityContext(ActorId sourceId, TargetInfo targetInfo, BattleEventBus eventBus, TargetingSystem targetingSystem)
    {
        SourceId = sourceId;
        TargetInfo = targetInfo;
        Events = eventBus;
        Targeting = targetingSystem;
    }
}