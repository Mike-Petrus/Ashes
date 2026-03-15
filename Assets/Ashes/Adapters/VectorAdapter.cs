using UnityEngine;

public static class VectorAdapter
{
    public static Vector3 ToUnity(SimVector3 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }

    public static SimVector3 ToSim(Vector3 v)
    {
        return new SimVector3(v.x, v.y, v.z);
    }
}