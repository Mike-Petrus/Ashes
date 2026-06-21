using System.Collections.Generic;

public class CursorMovedEvent : IBattleEvent
{
    public SimVector3 Position { get; }
    public bool IsVisible { get; }
    public bool IsValid { get; }

    public TargetingMode Mode{ get; }
    public float Radius { get; }
    public float Angle { get; }

    public List<SimVector3> Path { get; }

    public CursorMovedEvent(SimVector3 position, bool isVisible, bool isValid = true, TargetingMode mode = TargetingMode.SingleTarget, float radius = 0f, float angle = 0f, List<SimVector3> path = null)
    {
        Position = position;
        IsVisible = isVisible;
        IsValid = isValid;

        Mode = mode;
        Radius = radius;
        Angle = angle;
         
        Path = path ?? new List<SimVector3>();
    }
}