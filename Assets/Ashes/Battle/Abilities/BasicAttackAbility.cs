public class BasicAttackAbility : Ability
{
    public int Damage { get; } = 10;

    public BasicAttackAbility()
    {
        Name = "Attack";
        Range = 2f;
        Radius = 0f;
        Mode = TargetingMode.SingleTarget;
        Alignment = TargetAlignment.Everyone;
    }

    public override void Execute(AbilityContext context)
    {
        // 1. Ask the Targeting System who is in the attack zone
        var targets = context.Targeting.GetAffectedTargets(context.SourceId, context.TargetInfo, this);

        // 2. Fire the events for the EffectPipeline
        foreach (var targetId in targets)
        {
            context.Events.Publish(new DamageRequestEvent(context.SourceId, targetId, Damage));
        }
    }
}