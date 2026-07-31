public class ActiveStatus
{
    public string StatusId { get; }
    public string Name { get; }
    public bool IsBuff { get; }
    
    public float DurationLeft { get; set; }
    public float TickInterval { get; }
    public float TimeSinceLastTick { get; set; }
    
    public StatusTickType TickType { get; }
    public int Power { get; }

    public ActorId SourceId { get; }

    public int Stacks { get; set; }
    public int MaxStacks { get; }

    public string TriggerAbilityId { get; }         // The ability that triggers the interaction (e.g. spell_frostbolt)
    public string TriggerStatusId { get; }          // The status that is applied after detonation (e.g. status_frozen)
    public bool ConsumesStacks { get; }
    public bool RequireMaxStacks { get; }

    public ActiveStatus(StatusEffectTemplate template, float duration, int power, ActorId sourceId)
    {
        StatusId = template.StatusId;
        Name = template.StatusName;
        IsBuff = template.IsBuff;
        
        DurationLeft = duration;
        TickInterval = template.TickInterval;
        TimeSinceLastTick = 0f;
        
        TickType = template.TickType;
        Power = power;
        
        SourceId = sourceId;

        Stacks = 1;
        MaxStacks = template.MaxStacks;
        TriggerAbilityId = template.TriggerAbilityId;
        TriggerStatusId = template.TriggerStatusId;
        ConsumesStacks = template.ConsumesStacks;
        RequireMaxStacks = template.RequireMaxStacks;
    }
}