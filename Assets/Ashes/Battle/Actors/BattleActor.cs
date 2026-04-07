using System.Collections.Generic;

public class BattleActor
{
    public ActorId Id;
    public string Name;

    public CharacterStats Stats { get; private set; }
    public AbilitySet Abilities { get; private set; } = new();
    public int CurrentHP
    {
        get => Stats.CurrentHP;
        set => Stats.CurrentHP = System.Math.Clamp(value, 0, Stats.MaxHP);
    }
    public int CurrentMP
    {
        get => Stats.CurrentMP;
        set => Stats.CurrentMP = System.Math.Clamp(value, 0, Stats.MaxMP);
    }

    public float Speed => Stats.Speed;

    public float ATB;
    public float MaxATB = 100;

    public SimVector3 Position;
    public float Radius = 1.0f;

    public ActorState State = ActorState.Idle;
    public List<ActiveStatus> ActiveStatuses { get; } = new List<ActiveStatus>();

    public bool IsReady => ATB >= MaxATB;
    public bool IsAlive => State != ActorState.Dead && CurrentHP > 0;

    public BattleActor(ActorId id, string name, CharacterStats stats, SimVector3 position, float radius = 1.0f)
    {
        Id = id;
        Name = name;
        Stats = stats;
        Position = position;
        Radius = radius;
    }
}