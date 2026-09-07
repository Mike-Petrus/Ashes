public class BasicAttackAbility : Ability
{
    public int Damage { get; } = 10;

    public BasicAttackAbility()
    {
        AbilityId = "ability_attack_01";
        Name = "Attack";
        Category = "Weapon Skill";
        ImpactType = ImpactType.Damage;
        ElementType = ElementType.Physical;

        Range = 2f;
        Radius = 0f;
        Angle = 0f;
        RequiresLoS = true;

        Mode = TargetingMode.SingleTarget;
        Alignment = TargetAlignment.Everyone;
        CanTargetDead = false;
        RefundPercent = 0.25f;

        Effects.Add(new DamageEffect(10));
    }
}