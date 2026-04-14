using System.Collections.Generic;

public class MovementState
{
    public ActorId ActorId;
    public List<SimVector3> Waypoints;
    
    public int CurrentIndex;
    public float Progress; // 0 -> 1
}