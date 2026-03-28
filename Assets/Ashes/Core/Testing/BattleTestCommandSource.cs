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

        builder.AddStep(new MoveStep(actor, new SimVector3(2, 0, 0)));

        // Simple test target (self for now)
        ActorId target = actor;

        builder.AddStep(new AbilityStep(actor, new BasicAttackAbility(), target));

        BattleCommand command = builder.Build();
        
        if (validator.Validate(command))
        {
            events.Publish(new CommandBuiltEvent(command));            
        }
    }
}