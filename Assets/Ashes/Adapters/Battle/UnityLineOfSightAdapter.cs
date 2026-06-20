using UnityEngine;

public class UnityLineOfSightAdapter : ILineOfSightChecker
{
    private LayerMask obstacleLayer;

    public UnityLineOfSightAdapter(LayerMask obstacleLayer)
    {
        this.obstacleLayer = obstacleLayer;
    }

    public bool HasLineOfSight(SimVector3 origin, SimVector3 target)
    {
        Vector3 unityOrigin = new Vector3(origin.x, origin.y + 1f, origin.z);
        Vector3 unityTarget = new Vector3(target.x, target.y + 1f, target.z);

        Vector3 direction = unityTarget - unityOrigin;
        float distance = direction.magnitude;

        if (Physics.Raycast(unityOrigin, direction.normalized, distance, obstacleLayer))
        {
            return false;
        }

        return true;
    }
}