public class BattleCommandBuilder
{
    private BattleCommand currentCommand;

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

    // TODO: CancelStep -> ensure we can properly flow backwards through commands

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
}