using UnityEngine;

public class UnityNavMeshValidator : IMapValidator
{
    public bool IsValidPosition(SimVector3 position, float tolerance)
    {
        Vector3 unityPos = new Vector3(position.x, position.y, position.z);
        return UnityEngine.AI.NavMesh.SamplePosition(unityPos, out _, tolerance, UnityEngine.AI.NavMesh.AllAreas);
    }

    public SimVector3 GetNearestValidPosition(SimVector3 target, float searchRadius)
    {
        Vector3 unityPos = new Vector3(target.x, target.y, target.z);
        if (UnityEngine.AI.NavMesh.SamplePosition(unityPos, out var hit, searchRadius, UnityEngine.AI.NavMesh.AllAreas))
        {
            return new SimVector3(hit.position.x, hit.position.y, hit.position.z);
        }
        return target; // Fallback if absolutely no NavMesh is found
    }
}