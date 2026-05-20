using System.Collections.Generic;

public class BattleActor
{
    public ActorId Id { get; }
    public string Name { get; }

    public ActorFaction Faction { get; }
    public CharacterStats Stats { get; private set; }
    public AbilitySet Abilities { get; private set; } = new();

    public SimVector3 Position { get; set; }
    public float Radius { get; } = 1.0f;
    public ActorState State = ActorState.Idle;

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

    public List<ActiveStatus> ActiveStatuses { get; } = new List<ActiveStatus>();

    public bool IsReady => ATB >= MaxATB;
    public bool IsAlive => State != ActorState.Dead && CurrentHP > 0;

    public BattleActor(ActorId id, string name, CharacterStats stats, SimVector3 position, float radius = 1.0f, ActorFaction faction = ActorFaction.Party)
    {
        Id = id;
        Name = name;
        Stats = stats;
        Position = position;
        Radius = radius;
        Faction = faction;
    }
}