using UnityEngine;
using System;
using System.Collections.Generic;

public enum ConfigurableRequirementType { MP, HP, Item, RequiresStatus, RequiresAbsenceOfStatus }

[Serializable]
public class RequirementConfig
{
    public ConfigurableRequirementType Type;
    public int Amount;
    [Tooltip("Only required if Type is Item")]
    public string ItemId;
    [Tooltip("Required for Status checks (e.g. 'status_silence_01')")]
    public string StatusId;
}

[CreateAssetMenu(fileName = "NewAbilityTemplate", menuName = "Ashes/Data/Ability Template")]
public class AbilityTemplateSO : ScriptableObject
{
    [Header("Identity")]
    public string AbilityId;
    public string Name;
    public string Category = "Magic";

    [Header("Spatial Rules")]
    public float Range = 5f;
    public float Radius = 0f;
    public float Angle = 0f;
    public bool RequiresLoS = true;

    [Header("Targeting Rules")]
    public TargetingMode Mode = TargetingMode.SingleTarget;
    public TargetAlignment Alignment = TargetAlignment.Enemy;
    public float RefundPercent = 0.25f;

    [Header("Requirements (Costs)")]
    public List<RequirementConfig> Requirements = new List<RequirementConfig>();

    [Header("Effects")]
    public List<SharedEffectConfig> Effects = new List<SharedEffectConfig>();

    /// <summary>
    /// Converts this Unity asset into a pure C# domain blueprint.
    /// </summary>
    public AbilityTemplate ToDomain()
    {
        var template = new AbilityTemplate
        {
            AbilityId = this.AbilityId,
            Name = this.Name,
            Category = this.Category,
            Range = this.Range,
            Radius = this.Radius,
            Angle = this.Angle,
            RequiresLoS = this.RequiresLoS,
            Mode = this.Mode,
            Alignment = this.Alignment,
            RefundPercent = this.RefundPercent,
            Requirements = new List<AbilityRequirement>(),
            Effects = new List<Effect>()
        };

        // 1. Map Requirements
        foreach (var req in this.Requirements)
        {
            switch (req.Type)
            {
                case ConfigurableRequirementType.MP:
                    template.Requirements.Add(new MPCost(req.Amount));
                    break;
                case ConfigurableRequirementType.HP:
                    template.Requirements.Add(new HPCost(req.Amount));
                    break;
                case ConfigurableRequirementType.Item:
                    template.Requirements.Add(new ItemCost(req.ItemId, req.Amount));
                    break;
                case ConfigurableRequirementType.RequiresStatus:
                    template.Requirements.Add(new StatusCost(req.StatusId, true));
                    break;
                case ConfigurableRequirementType.RequiresAbsenceOfStatus:
                    template.Requirements.Add(new StatusCost(req.StatusId, false));
                    break;
            }
        }

        // 2. Map Effects (Using our incredibly handy SharedEffectConfig!)
        foreach (var config in this.Effects)
        {
            Effect domainEffect = config.ToDomainEffect();
            if (domainEffect != null)
            {
                template.Effects.Add(domainEffect);
            }
        }

        return template;
    }
}