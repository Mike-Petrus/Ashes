public struct CommandBuiltEvent
{
    public BattleCommand Command;

    public CommandBuiltEvent(BattleCommand command)
    {
        Command = command;
    }
}