public class BattleCommandExecutor : IBattleSystem
{
    private BattleContext context;

    private BattleCommand currentCommand;
    private int currentStepIndex;
    private CommandStep currentStep;

    public BattleCommandExecutor(BattleContext ctx)
    {
        context = ctx;
    }

    public bool IsExecuting => currentCommand != null;

    // TODO: Later systems (camera, animatio, UI) will need these events to react to
    // CommandStartedEvent, CommandFinishedEvent, CommandStepStartedEvent

    public void StartCommand(BattleCommand command)
    {
        if (currentCommand != null)
        {
            throw new System.Exception("Command already executing.");
        }

        if (command == null || command.Steps.Count == 0)
        {
            throw new System.Exception("Command has no steps.");
        }

        currentCommand = command;
        currentStepIndex = 0;

        context.Events.Publish(new CommandStartedEvent(command));

        StartCurrentStep();
    }

    private void StartCurrentStep()
    {
        if (currentStepIndex >= currentCommand.Steps.Count)
        {
            FinishCommand();
            return;
        }

        currentStep = currentCommand.Steps[currentStepIndex];

        if (currentStep == null)
        {
            throw new System.Exception($"Command step {currentStepIndex} is null.");
        }

        context.Events.Publish(new CommandStepStartedEvent(currentStep));

        currentStep.Start(context);

        // Handle steps that finish immediately
        if (currentStep.IsFinished)
        {
            AdvanceStep();
        }
    }

    public void Update(float deltaTime)
    {
        if (currentCommand == null)
        {
            return;
        }

        currentStep?.Update(deltaTime);     


        if (currentStep != null && currentStep.IsFinished)
        {
            AdvanceStep();
        }
    }

    private void AdvanceStep()
    {
        currentStepIndex++;
        StartCurrentStep();
    }

    private void FinishCommand()
    {
        context.Events.Publish(new CommandFinishedEvent(currentCommand));

        // TODO: Decide if this is the best place for this
        // Probably defer to ATB system with a ATBResetEvent
        // or just have it subscribe to CommandFinishedEvent
        // Should be fine in general, but may change with future edge cases
        context.Actors.GetActor(currentCommand.ActorId).ATB = 0;
        context.Events.Publish(new ATBChangedEvent(currentCommand.ActorId, 0f));
        
        currentCommand = null;
        currentStep = null;
    }
}