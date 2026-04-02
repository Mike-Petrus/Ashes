public class BattleArena
{
    public SimVector3 Center { get; }
    public float Radius { get; }

    // Don't know if we need these but for now we'll
    // keep them for testing and consistency
    private const float MinRadius = 15f;
    private const float MaxRadius = 30f;

    public BattleArena(SimVector3 center, int totalActors)
    {
        Center = center;
        
        // TODO: Test scaling with different party party sizes
        float calculatedRadius = 10f + (totalActors * 2.0f);

        Radius = System.Math.Clamp(calculatedRadius, MinRadius, MaxRadius);
    }

    public bool IsInBounds(SimVector3 position)
    {
        return SimVector3.Distance(Center, position) <= Radius;
    }

    public SimVector3 ClampToArena(SimVector3 targetPosition)
    {
        if (IsInBounds(targetPosition))
        {
            return targetPosition;
        }

        // Snap position to arena boundary 
        SimVector3 direction = (targetPosition - Center).Normalized();
        return Center + (direction * Radius);
    }
}