using System.Collections.Generic;

public class DummyAbility : Ability
{
    // DummyId was replaced with the general AbilityId
    // The id should describe the reason for the dummy

    public DummyAbility(
        string id,
        string name = "System Action",
        string category = "System",
        ImpactType impact = ImpactType.Neutral,
        ElementType element = ElementType.System,
        float range = 0f,
        float radius = 0f,
        float angle = 0f,
        bool requiresLoS = false,
        TargetingMode mode = TargetingMode.SingleTarget,
        TargetAlignment alignment = TargetAlignment.Everyone,
        bool canTargetDead = false)
    {
        AbilityId = id;
        Name = name;
        Category = category;
        ImpactType = impact;
        ElementType = element;
        
        Range = range;
        Radius = radius;
        Angle = angle;
        RequiresLoS = requiresLoS;
        
        Mode = mode;
        Alignment = alignment;
        CanTargetDead = canTargetDead;
        
        Requirements = new List<AbilityRequirement>();
        Effects = new List<Effect>();
        RefundPercent = 0.0f; 
    }
}