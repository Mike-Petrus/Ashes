public class BattleContext
{
    public BattleEventBus Events;

    public ActorRegistry Actors;

    public ActorStateSystem ActorStates;

    public MovementSystem Movement;

    public AbilitySystem Abilities;

    public RangeSystem Range;

    public CombatSystem Combat;

    public BattleClock Clock;
}