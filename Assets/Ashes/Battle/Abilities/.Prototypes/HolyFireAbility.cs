public class HolyFireAbility : Ability
{
    public HolyFireAbility()
    {
        Name = "Holy Fire";
        Category = "Wrath";
        Range = 10f; 
        Mode = TargetingMode.SingleTarget;
        Alignment = TargetAlignment.Enemy;

        // The self-damage/resource costs
        Requirements.Add(new HPCost(10));
        Requirements.Add(new MPCost(10));

        // The payload
        Effects.Add(new DamageEffect(35));
    }
}