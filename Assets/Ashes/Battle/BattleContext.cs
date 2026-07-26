public class BattleContext
{
    public BattleEventBus Events;

    public ActorRegistry Actors;
    public ActorStateSystem ActorStates;

    public MovementSystem Movement;
    public PositionSystem Position;
    public IPathfinder Path;

    public AbilitySystem Abilities;

    public SharedInventory Inventory;

    public RangeSystem Range;

    public EffectPipeline Effects;

    public BattleClock Clock;

    public IItemDatabase ItemDatabase;
    public IAbilityDatabase AbilityDatabase;
}