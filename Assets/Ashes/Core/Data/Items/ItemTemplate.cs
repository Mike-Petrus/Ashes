using System.Collections.Generic;

public class ItemTemplate
{
    public string ItemId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ItemType Type { get; set; }

    // Combat Properties
    public float Range { get; set; }
    public float Radius { get; set; } // 0 if not AoE
    public TargetingMode TargetMode { get; set; }
    public TargetAlignment Alignment { get; set; }

    public List<Effect> Effects { get; set; } = new List<Effect>();
}