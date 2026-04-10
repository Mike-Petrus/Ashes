public class CommandStartedEvent : IBattleEvent
{
    public BattleCommand Command;

    public CommandStartedEvent(BattleCommand command)
    {
        Command = command;
    }
}