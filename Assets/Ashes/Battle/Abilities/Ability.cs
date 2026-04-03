using System.Collections.Generic;

public abstract class Ability
{
    public string Name { get; protected set; }
    public List<Effect> Effects { get; protected set; } = new List<Effect>();

    // Spatial Rules
    public float Range { get; protected set; }
    public float Radius { get; protected set; } // 0 if not AoE

    // Targeting Rules
    public TargetingMode Mode { get; protected set; }
    public TargetAlignment Alignment { get; protected set;}

    // TODO: add bool CanTargetDead e.g. Resurrection
    
    public float RefundPercent { get; protected set; } = 0.25f;     // ATB % refunded if ability is canceled/mutated to WaitStep

    public virtual void Execute(AbilityContext context)
    {
        // 1. Ask the Targeting System who is in the attack zone
        var targets = context.Targeting.GetAffectedTargets(context.SourceId, context.TargetInfo, this);

        // 2. Fire the events for the EffectPipeline
        foreach (var targetId in targets)
        {
            var effectContext = new EffectContext(context.SourceId, targetId);
            context.Events.Publish(new EffectRequestEvent(effectContext, Effects));
        }
    }
}