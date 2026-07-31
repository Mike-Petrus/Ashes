using UnityEngine;
using System;
using System.Collections.Generic;

public enum ConfigurableEffectType { Heal, Damage, ApplyStatus, ATBModify, Cleanse, TeleportToTarget }

[Serializable]
public class SharedEffectConfig
{
    public ConfigurableEffectType EffectType;
    
    [Tooltip("The amount to heal, damage, the power of the status tick, OR the raw ATB amount")]
    public int Power;
    
    [Header("Status Overrides (Only if ApplyStatus)")]
    [Tooltip("Drag a StatusBlueprintSO here to guarantee a valid status is created.")]
    public StatusBlueprintSO StatusBlueprint;
    
    [Tooltip("Leave at 0 to use the blueprint's default duration.")]
    public float OverrideDuration;

    [Header("Cleanse Settings (Only if Cleanse)")]
    public bool CleanseAllDebuffs;
    public List<StatusBlueprintSO> SpecificStatusesToCleanse = new List<StatusBlueprintSO>();

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
        else if (EffectType == ConfigurableEffectType.ATBModify)
        {
            return new ATBModifyEffect(Power);
        }
        else if (EffectType == ConfigurableEffectType.TeleportToTarget)
        {
            return new TeleportToTargetEffect();
        }
        else if (EffectType == ConfigurableEffectType.ApplyStatus && StatusBlueprint != null)
        {
            // 1. Determine Duration
            float duration = OverrideDuration > 0 ? OverrideDuration : StatusBlueprint.DefaultDuration;

            // 2. We no longer build the payload here! We just pass the ID and Power to the engine.
            return new ApplyStatusEffect(StatusBlueprint.StatusId, duration, Power);
        }
        else if (EffectType == ConfigurableEffectType.Cleanse)
        {
            List<string> ids = new List<string>();
            foreach (var statusBP in SpecificStatusesToCleanse)
            {
                if (statusBP != null) ids.Add(statusBP.StatusId);
            }
            return new CleanseEffect(CleanseAllDebuffs, ids);
        }

        return null;
    }
}