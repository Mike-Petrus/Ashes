public class PursuitToggledEvent : IBattleEvent
{
    public bool IsEnabled { get; }

    public PursuitToggledEvent(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }
}