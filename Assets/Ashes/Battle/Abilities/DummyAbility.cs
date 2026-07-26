using System.Collections.Generic;

public class DummyAbility : Ability
{
    public string DummyId;     // Describes the reason for dummy

    public DummyAbility(
        string id,
        string name = "System Action",
        string category = "System",
        float range = 0f,
        float radius = 0f,
        float angle = 0f,
        TargetingMode mode = TargetingMode.SingleTarget,
        TargetAlignment alignment = TargetAlignment.Everyone,
        bool requiresLoS = false)
    {
        DummyId = id;
        Name = name;
        Category = category;
        
        Range = range;
        Radius = radius;
        Angle = angle;
        
        Mode = mode;
        Alignment = alignment;
        RequiresLoS = requiresLoS;
        
        Requirements = new List<AbilityRequirement>();
        Effects = new List<Effect>();
        RefundPercent = 0.0f; 
    }
}