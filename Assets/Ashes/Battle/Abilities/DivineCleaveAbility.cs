public class DivineCleaveAbility : Ability
{
    public DivineCleaveAbility()
    {
        Name = "Divine Cleave";
        Category = "Wrath";
        Radius = 5f; // Massive 6-meter radius
        Angle = 120f;
        Mode = TargetingMode.Directional; // Can snap to actors OR free aim!
        Alignment = TargetAlignment.Enemy;
        RequiresLoS = true;
        RefundPercent = 0.25f;

        Requirements.Add(new MPCost(15));
        Effects.Add(new DamageEffect(30));
    }
}