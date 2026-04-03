using System.Collections.Generic;

public class BattleActor
{
    public ActorId Id;
    public string Name;

    public float Health = 100;
    public float Speed;
    
    public float ATB;
    public float MaxATB = 100;

    public SimVector3 Position;
    public float Radius = 1.0f;

    public ActorState State = ActorState.Idle;
    public List<ActiveStatus> ActiveStatuses { get; } = new List<ActiveStatus>();

    public bool IsReady => ATB >= MaxATB;
    public bool IsAlive => State != ActorState.Dead;

    public BattleActor(ActorId id, string name, float speed, SimVector3 position, float radius = 1.0f)
    {
        Id = id;
        Name = name;
        Speed = speed;
        Position = position;
        Radius = radius;
    }
}