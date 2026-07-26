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

        if (currentCommand.IsPursuit)
        {
            EvaluatePursuitCommand(command);
        }

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
        
        currentCommand = null;
        currentStep = null;
    }

    private void EvaluatePursuitCommand(BattleCommand command)
    {
        BattleActor sourceActor = context.Actors.GetActor(command.ActorId);
        BattleActor targetActor;
        SimVector3 destination;
        TargetInfo targetInfo;

        // 1. Extract steps to determine intent
        MoveStep ogMove = null;
        AbilityStep ogAbility = null;

        foreach (CommandStep step in command.Steps)
        {
            if (step is MoveStep moveStep)
            {
                ogMove = moveStep;
            }
            if (step is AbilityStep abilityStep)
            {
                ogAbility = abilityStep;
            }
        }

        // This should never really happen... Unless we change "Wait" to "Follow"
        // when Pursuit = ON in Phase 2
        if (ogAbility == null)
        {
            destination = ogMove != null ? ogMove.Destination : sourceActor.Position;
        }
        
        // ---- package regular move + wait ; return;

        targetInfo = ogAbility.TargetInfo;

        if (targetInfo.TargetActor.HasValue)
        {
            targetActor = context.Actors.GetActor(targetInfo.TargetActor.Value);
            destination = targetActor.Position;
        }
    }
}