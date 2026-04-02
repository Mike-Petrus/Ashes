public class BattleTestCommandSource
{
    private BattleEventBus events;
    private BattleCommandBuilder builder;
    private CommandValidator validator;

    public BattleTestCommandSource(BattleEventBus eventBus)
    {
        events = eventBus;
        builder = new BattleCommandBuilder();

        // TODO: Validator should probably be built in BattleSimulation
        // but for now this is our only command source so this is fine
        validator = new CommandValidator();

        events.Subscribe<ActorReadyEvent>(OnActorReady);
    }

    private void OnActorReady(ActorReadyEvent e)
    {
        ActorId actor = e.ActorId;

        builder.BeginCommand(actor);

        // 1. Give them slightly offset destinations so they don't all run to the exact same (2,0,0) spot
        SimVector3 destination = new SimVector3(actor.Value * 2f, 0, 5f);
        builder.AddStep(new MoveStep(actor, destination));

        // 2. Let's make them actually fight each other!
        // If Actor is 1 or 2 (Knight/Mage), target the Goblin (3). 
        // If Actor is 3 (Goblin), target the Knight (1).
        ActorId targetId = actor.Value <= 2 ? new ActorId(3) : new ActorId(1);

        // 3. THE FIX: Wrap the ActorId in our new TargetInfo payload!
        TargetInfo targetInfo = TargetInfo.ForActor(targetId);

        // Now pass targetInfo into the AbilityStep instead of the raw ActorId
        builder.AddStep(new AbilityStep(actor, new BasicAttackAbility(), targetInfo));

        BattleCommand command = builder.Build();
        
        if (validator.Validate(command))
        {
            events.Publish(new CommandBuiltEvent(command));            
        }
    }
}