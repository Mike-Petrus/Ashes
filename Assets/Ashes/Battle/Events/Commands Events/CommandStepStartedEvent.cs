public class CommandStepStartedEvent : IBattleEvent
{
    public CommandStep Step;

    public CommandStepStartedEvent(CommandStep step)
    {
        Step = step;
    }
}