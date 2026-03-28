public class CommandFinishedEvent : IBattleEvent
{
    public BattleCommand Command;

    public CommandFinishedEvent(BattleCommand command)
    {
        Command = command;
    }
}