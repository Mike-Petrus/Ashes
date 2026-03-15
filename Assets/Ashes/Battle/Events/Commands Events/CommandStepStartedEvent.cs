public struct CommandStepStartedEvent
{
    public CommandStep Step;

    public CommandStepStartedEvent(CommandStep step)
    {
        Step = step;
    }
}