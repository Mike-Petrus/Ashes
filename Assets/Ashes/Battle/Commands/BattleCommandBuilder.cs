using System.Collections.Generic;

public class BattleCommandBuilder
{
    private BattleCommand currentCommand;

    public int Size => currentCommand != null ? currentCommand.Steps.Count : 0;
    public IReadOnlyList<CommandStep> Steps => currentCommand.Steps;

    public void BeginCommand(ActorId actorId)
    {
        currentCommand = new BattleCommand(actorId);
    }

    public void AddStep(CommandStep step)
    {
        if (currentCommand == null)
        {
            throw new System.Exception("No command started.");
        }
        currentCommand.AddStep(step);
    }

    public void UndoLastStep()
    {
        currentCommand.RemoveLastStep();
    }

    public BattleCommand Build()
    {
        if (currentCommand == null)
        {
            throw new System.Exception("No command to build.");
        }

        BattleCommand finished = currentCommand;
        currentCommand = null;
        return finished;
    }

    public void Cancel()
    {
        currentCommand = null;
    }

    public CommandStep LastStepAdded()
    {
        return currentCommand.LastStep();
    }
}