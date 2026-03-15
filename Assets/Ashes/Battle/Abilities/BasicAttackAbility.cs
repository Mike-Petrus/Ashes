public class BasicAttackAbility : Ability
{
    public int Damage { get; } = 10;

    public BasicAttackAbility()
    {
        Name = "Attack";
        Range = 2f;
    }

    public override void Execute(AbilityContext context)
    {
        context.Events.Publish(new DamageRequestEvent(context.SourceId, context.TargetId, Damage));
    }
}