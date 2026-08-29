using System.Collections.Generic;

public abstract class Ability
{
    // Identity
    public string AbilityId { get; protected set; }
    public string Name { get; protected set; }

    // Core Properties
    public string Category { get; protected set; }
    public ImpactType ImpactType { get; protected set; }
    public ElementType ElementType { get; protected set; }

    // Spatial Rules
    public float Range { get; protected set; }
    public float Radius { get; protected set; } // 0 if not AoE
    public float Angle { get; protected set; } // for cone angle
    public bool RequiresLoS { get; protected set; } = true; 

    // Targeting Rules
    public TargetingMode Mode { get; protected set; }
    public TargetAlignment Alignment { get; protected set;}
    public bool CanTargetDead { get; protected set; } = false;
    public float RefundPercent { get; protected set; } = 0.25f;     // ATB % refunded if ability is canceled/mutated to WaitStep
    
    public List<Effect> Effects { get; protected set; } = new();
    public List<AbilityRequirement> Requirements { get; protected set; } = new();

    public virtual void Execute(AbilityContext context)
    {
        // 1. Pay the Ability Cost
        foreach (var req in Requirements)
        {
            req.ConsumeRequirement(context);
        }
        
        // 2. Ask the Targeting System who is in the attack zone
        var targets = context.Targeting.GetAffectedTargets(context.SourceId, context.TargetInfo, this);

        // 3. Fire the events for the EffectPipeline
        foreach (var targetId in targets)
        {
            var effectContext = new EffectContext(context.SourceId, targetId);
            context.Events.Publish(new EffectRequestEvent(effectContext, Effects));
        }
    }
}