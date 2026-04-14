using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshPathfinder : MonoBehaviour, IPathfinder
{
    public List<SimVector3> FindPath(SimVector3 start, SimVector3 destination, float actorRadius)
    {
        // Edge case: Micro-movement with the actor's own radius
        if (SimVector3.Distance(start, destination) <= actorRadius)
        {
            return new List<SimVector3> { start, destination };
        }

        // 1. Translate from Sim Space to Unity Space
        Vector3 unityStart = VectorAdapter.ToUnity(start);
        Vector3 unityDestination = VectorAdapter.ToUnity(destination);

        Vector3 directionToTarget = (unityDestination - unityStart).normalized;
        Vector3 escapePoint = unityStart + (directionToTarget * (actorRadius * 1.5f));

        if (NavMesh.SamplePosition(escapePoint, out NavMeshHit startHit, actorRadius * 2.0f, NavMesh.AllAreas))
        {
            unityStart = startHit.position;
        }

        // Y-axis floor snap
        if (NavMesh.SamplePosition(unityDestination, out NavMeshHit destHit, 0.1f, NavMesh.AllAreas))
        {
            unityDestination = destHit.position;
        }

        NavMeshPath path = new NavMeshPath();
        List<SimVector3> waypoints = new List<SimVector3>();

        // 2. Ask Unity's NavMesh to calculate the route
        if (NavMesh.CalculatePath(unityStart, unityDestination, NavMesh.AllAreas, path))
        {
            // Ensure the path actually reaches the destination (not just a partial path)
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                waypoints.Add(start);
                // 3. Translate the corners back into Sim Space
                foreach (Vector3 corner in path.corners)
                {
                    SimVector3 simCorner = VectorAdapter.ToSim(corner);
                    if (SimVector3.Distance(waypoints[waypoints.Count - 1], simCorner) > 0.01f)
                    {
                        waypoints.Add(simCorner);
                    }
                }

                return waypoints;
            }
        }

        // Completely unreachable
        return new List<SimVector3>();
    }
}