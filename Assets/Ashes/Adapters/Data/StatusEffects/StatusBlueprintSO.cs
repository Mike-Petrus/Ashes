using UnityEngine;
using System;
using System.Collections.Generic;

// The visual config for the Unity Inspector
[Serializable]
public class StatModifierConfig
{
    public StatType Stat;
    public float FlatValue;
    
    [Tooltip("Use decimals (e.g., 0.5 for +50%, -0.25 for -25%)")]
    public float PercentValue;
}

[CreateAssetMenu(fileName = "NewStatusBlueprint", menuName = "Ashes/Data/Status Blueprint")]
public class StatusBlueprintSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("The exact string ID the engine recognizes (e.g., 'status_poison', 'status_haste')")]
    public string StatusId;
    public string StatusName;
    public bool IsBuff;

    [Header("Timing")]
    public float DefaultDuration = 15f;
    [Tooltip("How often this effect ticks in seconds. Use 0 for passive buffs like Haste.")]
    public float TickInterval = 3f;

    [Header("Tick Behavior")]
    [Tooltip("What type of payload should be generated when this status ticks?")]
    public StatusTickType TickType = StatusTickType.Damage;

    [Header("Stacking & Triggers")]
    [Tooltip("How many times can this status stack? (1 = no stacking)")]
    public int MaxStacks = 1;
    [Tooltip("The ID of the ability that detonates this status (e.g., 'spell_frostbolt')")]
    public AbilityTemplateSO TriggerAbilityId;
    [Tooltip("The status to apply when triggered (e.g., Frozen)")]
    public StatusBlueprintSO TriggerStatusBlueprint;
    public bool RequireMaxStacks = true;
    public bool ConsumesStacks = true;

    [Header("Stat Modifiers")]
    [Tooltip("Passive stat changes applied while this status is active.")]
    public List<StatModifierConfig> StatModifiers = new List<StatModifierConfig>();
    
    /// <summary>
    /// Converts this Unity asset into a pure C# domain template.
    /// </summary>
    public StatusEffectTemplate ToDomain()
    {
        var template = new StatusEffectTemplate
        {
            StatusId = this.StatusId,
            StatusName = this.StatusName,
            IsBuff = this.IsBuff,
            DefaultDuration = this.DefaultDuration,
            TickInterval = this.TickInterval,
            TickType = this.TickType,
            MaxStacks = this.MaxStacks,
            TriggerAbilityId = this.TriggerAbilityId != null ? this.TriggerAbilityId.AbilityId : null,
            TriggerStatusId = this.TriggerStatusBlueprint != null ? this.TriggerStatusBlueprint.StatusId : null,
            ConsumesStacks = this.ConsumesStacks,
            RequireMaxStacks = this.RequireMaxStacks
        };

        foreach (var mod in this.StatModifiers)
        {
            template.StatModifiers.Add(new StatModifierTemplate
            {
                Stat = mod.Stat,
                FlatValue = mod.FlatValue,
                PercentValue = mod.PercentValue
            });
        }

        return template;
    }
}