using System.Collections.Generic;

public class CursorMovedEvent : IBattleEvent
{
    public SimVector3 Position;
    public bool IsVisible;
    public bool IsValid;
    public List<SimVector3> Path { get; }

    public CursorMovedEvent(SimVector3 position, bool isVisible, bool isValid = true, List<SimVector3> path = null)
    {
        Position = position;
        IsVisible = isVisible;
        IsValid = isValid;
        Path = path ?? new List<SimVector3>();
    }
}