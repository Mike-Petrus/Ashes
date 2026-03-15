public class BattleContext
{
    public EventBus Events;

    public ActorRegistry Actors;

    public ActorStateSystem ActorStates;

    public MovementSystem Movement;

    public AbilitySystem Abilities;

    public CombatSystem Combat;

    public BattleClock Clock;
}