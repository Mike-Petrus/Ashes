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

        Effects.Add(new DamageEffect(10));
    }
}