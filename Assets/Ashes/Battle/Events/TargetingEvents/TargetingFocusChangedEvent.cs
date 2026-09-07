public class TargetingFocusChangedEvent : IBattleEvent
{
    public ActorId? FocusedTargetId { get; }

    public TargetingFocusChangedEvent(ActorId? focusedTargetId)
    {
        FocusedTargetId = focusedTargetId;
    }
}