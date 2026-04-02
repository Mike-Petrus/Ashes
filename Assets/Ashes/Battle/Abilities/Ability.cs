using System.Collections.Generic;

public abstract class Ability
{
    public string Name { get; protected set; }

    // Spatial Rules
    public float Range { get; protected set; }
    public float Radius { get; protected set; } // 0 if not AoE

    // Targeting Rules
    public TargetingMode Mode { get; protected set; }
    public TargetAlignment Alignment { get; protected set;}

    // TODO: add bool CanTargetDead e.g. Resurrection
    
    public float RefundPercent { get; protected set; } = 0.25f;     // ATB % refunded if ability is canceled/mutated to WaitStep

    public abstract void Execute(AbilityContext context);
}