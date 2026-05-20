public class BattleArena
{
    public SimVector3 Center { get; private set; }
    public float Radius { get; }

    // Hemisphere Data
    public SimVector3 PlayerFacingDir { get; }
    public SimVector3 DivisionAxis { get; }

    // Don't know if we need these but for now we'll
    // keep them for testing and consistency
    private const float MinRadius = 15f;
    private const float MaxRadius = 30f;

    public BattleArena(SimVector3 rawCenter, SimVector3 playerCollisionDir, int totalActors, IMapValidator mapValidator)
    {
        // 1. Calculate Arena Radius
        
        // TODO: Test scaling with different party party sizes
        float calculatedRadius = 10f + (totalActors * 2.0f);
        Radius = System.Math.Clamp(calculatedRadius, MinRadius, MaxRadius);

        // 2. Establish Hemisphere Vectors
        PlayerFacingDir = new SimVector3(playerCollisionDir.x, 0, playerCollisionDir.z).Normalized();

        // Calculate perpindicular axis - (FacingDir cross Y-Axis) = (-Facing.z, 0, Facing.x)
        DivisionAxis = new SimVector3(-PlayerFacingDir.z, 0, PlayerFacingDir.x).Normalized();

        // 3. Validate and Adjust Center
        // If raw center is hanging off a cliff, the MapValidator will push it into a valid space
        Center = mapValidator.GetNearestValidPosition(rawCenter, Radius);
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

    public SimVector3 GetPartyBaseLine()
    {
        // Puts the party slightly behind the center division line
        return Center - (PlayerFacingDir * 2f);
    }
}