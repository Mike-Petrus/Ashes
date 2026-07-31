using System.Collections.Generic;

public class StatusEffectTemplate
{
    public string StatusId { get; set; }
    public string StatusName { get; set; }
    public bool IsBuff { get; set; }

    public float DefaultDuration { get; set; }
    public float TickInterval { get; set; }
    public StatusTickType TickType { get; set; }

    public int MaxStacks { get; set; } = 1;
    public string TriggerAbilityId { get; set; }
    public string TriggerStatusId { get; set; }
    public bool ConsumesStacks { get; set; } = true;
    public bool RequireMaxStacks { get; set; } = true;

    public List<StatModifierTemplate> StatModifiers { get; set; } = new List<StatModifierTemplate>();
}