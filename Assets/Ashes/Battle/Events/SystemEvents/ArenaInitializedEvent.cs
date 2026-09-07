public class ArenaInitializedEvent : IBattleEvent
{
    public SimVector3 Center { get; }
    public float Radius { get; }

    public ArenaInitializedEvent(SimVector3 center, float radius)
    {
        Center = center;
        Radius = radius;
    }
}