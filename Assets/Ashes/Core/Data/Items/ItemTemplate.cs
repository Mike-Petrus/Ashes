using System.Collections.Generic;

public class ItemTemplate
{
    public string ItemId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    public ItemType Type { get; set; }
    public ImpactType Impact { get; set;}
    public ElementType Element { get; set; }

    // Combat Properties
    public float Range { get; set; }
    public float Radius { get; set; } // 0 if not AoE
    public float Angle { get; set; }  // 0 if not Cone
    public bool RequiresLoS { get; set; }

    public TargetingMode TargetMode { get; set; }
    public TargetAlignment Alignment { get; set; }
    public bool CanTargetDead { get; set; }

    public List<Effect> Effects { get; set; } = new List<Effect>();
}