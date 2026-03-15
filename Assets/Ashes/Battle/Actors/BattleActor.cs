public class BattleActor
{
    // TODO: replace raw ActorId with strongly typed ActorID struct
    public ActorId Id;
    public string Name;

    public float Health = 100;
    public float Speed;
    
    public float ATB;
    public float MaxATB = 100;

    public SimVector3 Position;

    public ActorState State = ActorState.Idle;

    public bool IsReady => ATB >= MaxATB;
    public bool IsAlive => State != ActorState.Dead;

    public BattleActor(ActorId id, string name, float speed, SimVector3 position)
    {
        Id = id;
        Name = name;
        Speed = speed;
        Position = position;
    }
}