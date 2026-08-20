using System.Collections.Generic;

public class AbilityTemplate
{
    public string AbilityId { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public ImpactType Impact { get; set; }
    public ElementType Element { get; set; }

    // Spatial Rules
    public float Range { get; set; }
    public float Radius { get; set; } 
    public float Angle { get; set; }
    public bool RequiresLoS { get; set; } = true;

    // Targeting Rules
    public TargetingMode Mode { get; set; }
    public TargetAlignment Alignment { get; set; }
    public bool CanTargetDead { get; set; } = false;
    public float RefundPercent { get; set; } = 0.25f;

    // We use the pure domain Effect classes here!
    public List<AbilityRequirement> Requirements { get; set; } = new List<AbilityRequirement>();
    public List<Effect> Effects { get; set; } = new List<Effect>();
}