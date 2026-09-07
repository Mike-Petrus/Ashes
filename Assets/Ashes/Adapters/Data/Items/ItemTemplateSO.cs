using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewItemTemplate", menuName = "Ashes/Data/Item Template")]
public class ItemTemplateSO : ScriptableObject
{
    [Header("Identity")]
    public string ItemId;
    public string Name;
    public Sprite Icon;
    [TextArea]
    public string Description;

    [Header("Core Properties")]
    public ItemType Type;
    public ImpactType Impact;
    public ElementType Element;

    [Header("Spatial Properties")]
    public float Range = 5f;
    [Tooltip("Size of AoE blast radius - SET TO ZERO IF NOT AOE/DIRECTIONAL")]
    public float Radius = 0f;
    [Tooltip("Size of Directional blast angle - SET TO ZERO IF NOT DIRECTIONAL")]
    public float Angle = 0f;
    public bool RequiresLoS = true;

    [Header("Targeting Properties")]
    public TargetingMode TargetMode = TargetingMode.SingleTarget;
    public TargetAlignment Alignment = TargetAlignment.Ally;
    public bool CanTargetDead = false;

    [Header("Effects")]
    [Tooltip("The effects that trigger when this item is used.")]
    public List<SharedEffectConfig> Effects = new List<SharedEffectConfig>();

    /// <summary>
    /// Converts this Unity asset into a pure C# domain object.
    /// </summary>
    public ItemTemplate ToDomain()
    {
        var template = new ItemTemplate
        {
            ItemId = this.ItemId,
            Name = this.Name,
            Description = this.Description,
            Type = this.Type,
            Impact = this.Impact,
            Element = this.Element,
            Range = this.Range,
            Radius = this.Radius,
            Angle = this.Angle,
            RequiresLoS = this.RequiresLoS,
            TargetMode = this.TargetMode,
            Alignment = this.Alignment,
            CanTargetDead = this.CanTargetDead,
            Effects = new List<Effect>()
        };

        // Convert all Unity Inspector configs into pure C# logic
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