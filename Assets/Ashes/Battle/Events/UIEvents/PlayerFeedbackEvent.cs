public class PlayerFeedbackEvent : IBattleEvent
{
    public string FeedbackMessage { get; private set; }
    // TODO: public enum MessageType MessageType { get; private set; }      If we want to specify different types in the future

    public PlayerFeedbackEvent(string message)
    {
        FeedbackMessage = message;
    }
}