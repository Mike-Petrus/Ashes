public class AbilityContext
{
    public ActorId SourceId;
    public ActorId TargetId;

    public EventBus Events;

    public AbilityContext(ActorId sourceId, ActorId targetId, EventBus eventBus)
    {
        SourceId = sourceId;
        TargetId = targetId;
        Events = eventBus;
    }
}