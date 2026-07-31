public class SacrificeAbility : Ability
{
    public SacrificeAbility()
    {
        Name = "Sacrifice";
        Category = "White Magic";
        Range = 5f;
        Mode = TargetingMode.SingleTarget;
        Alignment = TargetAlignment.Ally;

        // Costs BOTH HP and MP!
        Requirements.Add(new HPCost(30)); 
        Requirements.Add(new MPCost(10));

        Effects.Add(new HealEffect(100));    
    }
}