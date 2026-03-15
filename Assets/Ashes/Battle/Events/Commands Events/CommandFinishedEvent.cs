public struct CommandFinishedEvent
{
    public BattleCommand Command;

    public CommandFinishedEvent(BattleCommand command)
    {
        Command = command;
    }
}