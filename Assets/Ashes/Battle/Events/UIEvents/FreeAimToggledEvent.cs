public class FreeAimToggledEvent : IBattleEvent
{
    public bool IsEnabled { get; }

    public FreeAimToggledEvent(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }
}