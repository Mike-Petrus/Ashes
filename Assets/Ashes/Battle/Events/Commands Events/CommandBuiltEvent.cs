public class CommandBuiltEvent : IBattleEvent
{
    public BattleCommand Command;

    public CommandBuiltEvent(BattleCommand command)
    {
        Command = command;
    }
}