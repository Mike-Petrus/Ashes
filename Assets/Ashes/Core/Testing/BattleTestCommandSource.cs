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

        SimVector3 destination = new SimVector3(actor.Value * 2f, 0, 5f);
        builder.AddStep(new MoveStep(actor, destination));

        ActorId targetId = actor.Value <= 2 ? new ActorId(3) : new ActorId(1);
        TargetInfo targetInfo = TargetInfo.ForActor(targetId);

        // Assign the Ability based on who is acting!
        Ability abilityToCast = (actor.Value == 3) ? new PoisonDartAbility() : new BasicAttackAbility();

        builder.AddStep(new AbilityStep(actor, abilityToCast, targetInfo));

        BattleCommand command = builder.Build();
        
        if (validator.Validate(command))
        {
            events.Publish(new CommandBuiltEvent(command));            
        }
    }
}