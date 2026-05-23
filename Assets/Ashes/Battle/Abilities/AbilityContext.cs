public class AbilityContext
{
    public ActorId SourceId;
    public TargetInfo TargetInfo;

    public BattleEventBus Events;
    public ActorRegistry Actors;
    public SharedInventory Inventory;
    public TargetingSystem Targeting;

    public AbilityContext(ActorId sourceId, TargetInfo targetInfo, BattleEventBus eventBus, ActorRegistry actorRegistry, SharedInventory inventory, TargetingSystem targetingSystem)
    {
        SourceId = sourceId;
        TargetInfo = targetInfo;
        Events = eventBus;
        Actors = actorRegistry;
        Inventory = inventory;
        Targeting = targetingSystem;
    }
}