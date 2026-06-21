public class CometAbility : Ability
{
    public CometAbility()
    {
        Name = "Comet";
        Category = "White Magic";
        Range = 20f;
        Radius = 6f; // Massive 6-meter radius
        Mode = TargetingMode.HybridAoE; // Can snap to actors OR free aim!
        Alignment = TargetAlignment.Enemy;
        RequiresLoS = true;
        RefundPercent = 0.25f;

        Requirements.Add(new MPCost(30));
        Effects.Add(new DamageEffect(60));
    }
}