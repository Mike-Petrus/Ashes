public class AbilitySystem
{
    private BattleEventBus events;
    private ActorRegistry actors;
    private ActorStateSystem actorStates;
    private SharedInventory inventory;
    private TargetingSystem targeting;

    public AbilitySystem(BattleEventBus eventBus, ActorRegistry actorRegistry, ActorStateSystem states, SharedInventory sharedInventory, TargetingSystem targetingSystem)
    {
        events = eventBus;
        actors = actorRegistry;
        actorStates = states;
        inventory = sharedInventory;
        targeting = targetingSystem;

        events.Subscribe<AbilityRequestEvent>(OnAbilityRequest);
    }

    private void OnAbilityRequest(AbilityRequestEvent e)
    {
        actorStates.SetState(e.ActorId, ActorState.Acting);

        // AbilityStartedEvent -> animations

        AbilityContext ctx = new AbilityContext(e.ActorId, e.TargetInfo, events, actors, inventory, targeting);

        e.Ability.Execute(ctx);

        // Damage, effects, etc.
        // AbilityResolvedEvent

        actorStates.SetState(e.ActorId, ActorState.Idle);

        // Right now this fires immediately
        // TODO: Eventually we want AbilityStarted event -> Animation/FX -> AbilityResolvedEvent
        // This will fix the debug logging issues as well
        events.Publish(new AbilityCompletedEvent(e.ActorId, e.Ability, e.TargetInfo));
    }
}