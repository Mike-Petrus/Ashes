using System.Collections.Generic;

public class Item
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public ItemType Type { get; }

    // Combat Properties
    public float Range { get; }
    public float Radius { get; } // 0 if not AoE
    public TargetingMode TargetMode { get; }
    public TargetAlignment Alignment { get; }
    public List<Effect> Effects { get; }

    public Item(string id, string name, string description, ItemType type, float range, float radius, TargetingMode targetMode, TargetAlignment alignment, List<Effect> effects)
    {
        Id = id;
        Name = name;
        Description = description;
        Type = type;
        Range = range;
        Radius = radius;
        TargetMode = targetMode;
        Alignment = alignment;
        Effects = effects;
    }
}