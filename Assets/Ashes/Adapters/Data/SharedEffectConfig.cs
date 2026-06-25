using UnityEngine;
using System;
using System.Collections.Generic;

public enum ConfigurableEffectType { Heal, Damage, ApplyStatus }

[Serializable]
public class SharedEffectConfig
{
    public ConfigurableEffectType EffectType;
    
    [Tooltip("The amount to heal, damage, OR the power of the status tick!")]
    public int Power;
    
    [Header("Status Overrides (Only if ApplyStatus)")]
    [Tooltip("Drag a StatusBlueprintSO here to guarantee a valid status is created.")]
    public StatusBlueprintSO StatusBlueprint;
    
    [Tooltip("Leave at 0 to use the blueprint's default duration.")]
    public float OverrideDuration;

    /// <summary>
    /// Acts as a Factory: Converts this Unity Inspector config into a pure C# Effect instance.
    /// </summary>
    public Effect ToDomainEffect()
    {
        if (EffectType == ConfigurableEffectType.Heal)
        {
            return new HealEffect(Power);
        }
        else if (EffectType == ConfigurableEffectType.Damage)
        {
            return new DamageEffect(Power);
        }
        else if (EffectType == ConfigurableEffectType.ApplyStatus && StatusBlueprint != null)
        {
            // 1. Determine Duration
            float duration = OverrideDuration > 0 ? OverrideDuration : StatusBlueprint.DefaultDuration;

            // 2. Build the specific payload for this cast using the Power slider!
            List<Effect> tickPayload = new List<Effect>();
            if (StatusBlueprint.TickType == StatusTickType.Damage)
            {
                tickPayload.Add(new DamageEffect(Power));
            }
            else if (StatusBlueprint.TickType == StatusTickType.Heal)
            {
                tickPayload.Add(new HealEffect(Power));
            }

            // 3. Return the fully self-contained domain object
            return new ApplyStatusEffect(StatusBlueprint.StatusName, duration, StatusBlueprint.TickInterval, tickPayload);
        }

        return null;
    }
}