public class AbilityContext
{
    public ActorId SourceId;
    public ActorId TargetId;

    public BattleEventBus Events;

    public AbilityContext(ActorId sourceId, ActorId targetId, BattleEventBus eventBus)
    {
        SourceId = sourceId;
        TargetId = targetId;
        Events = eventBus;
    }
}