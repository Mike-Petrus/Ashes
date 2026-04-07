public class FrostBoltAbility : Ability
{
    public FrostBoltAbility()
    {
        Name = "Frost Bolt";
        Category = "Black Magic";
        Range = 10f;
        Mode = TargetingMode.SingleTarget;
        Alignment = TargetAlignment.Enemy;

        Requirements.Add(new MPCost(20));

        Effects.Add(new DamageEffect(15));
        Effects.Add(new ApplyStatusEffect("Slow", 10, 50));
    }
}