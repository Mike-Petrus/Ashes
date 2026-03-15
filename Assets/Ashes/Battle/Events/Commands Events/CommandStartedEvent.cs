public struct CommandStartedEvent
{
    public BattleCommand Command;

    public CommandStartedEvent(BattleCommand command)
    {
        Command = command;
    }
}