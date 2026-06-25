using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewItemTemplate", menuName = "Ashes/Data/Item Template")]
public class ItemTemplateSO : ScriptableObject
{
    [Header("Identity")]
    public string ItemId;
    public string Name;
    [TextArea]
    public string Description;
    public ItemType Type;

    [Header("Targeting Properties")]
    public float Range = 5f;
    public float Radius = 0f;
    public TargetingMode TargetMode = TargetingMode.SingleTarget;
    public TargetAlignment Alignment = TargetAlignment.Ally;

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
            Range = this.Range,
            Radius = this.Radius,
            TargetMode = this.TargetMode,
            Alignment = this.Alignment,
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