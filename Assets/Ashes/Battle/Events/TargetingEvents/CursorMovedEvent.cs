using System.Collections.Generic;

public class CursorMovedEvent : IBattleEvent
{
    public SimVector3 Position { get; }
    public bool IsVisible { get; }
    public bool IsValid { get; }

    public TargetingMode Mode { get; }
    public float Radius { get; }
    public float Angle { get; }

    public List<SimVector3> Path { get; }
    public SimVector3? StaticCenter { get; }    // Optional anchor point e.g. Directional abilities

    public CursorMovedEvent(SimVector3 position, bool isVisible, bool isValid = true, TargetingMode mode = TargetingMode.SingleTarget, float radius = 0f, float angle = 0f, List<SimVector3> path = null, SimVector3? staticCenter = null)
    {
        Position = position;
        IsVisible = isVisible;
        IsValid = isValid;

        Mode = mode;
        Radius = radius;
        Angle = angle;
         
        Path = path ?? new List<SimVector3>();
        StaticCenter = staticCenter;
    }
}