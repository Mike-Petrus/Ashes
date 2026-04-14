using System.Collections.Generic;

public interface IPathfinder
{
    /// <summary>
    /// Calculates a path from the start position to the target position
    /// Returns a list of waypoints. Returns an empty list or null if the path is invalid/blocked
    /// </summary>
    List<SimVector3> FindPath(SimVector3 start, SimVector3 destination, float actorRadius);
}