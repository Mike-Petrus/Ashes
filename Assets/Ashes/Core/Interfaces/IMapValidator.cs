public interface IMapValidator
{
    // Asks the adapter "Is this point on the NavMesh?"
    bool IsValidPosition(SimVector3 position, float tolerance);

    // Asks the adapter "If this point is off the cliff, where is the closest valid spot?"
    SimVector3 GetNearestValidPosition(SimVector3 target, float searchRadius);
}