public class HolyNovaAbility : Ability
{
    public HolyNovaAbility()
    {
        Name = "Holy Nova";
        Category = "Wrath";
        Range = 15f;
        Radius = 4f; // 4-meter radius explosion
        Mode = TargetingMode.ActorAoE; // Locked to actors
        Alignment = TargetAlignment.Enemy;
        RequiresLoS = true;
        RefundPercent = 0.25f;

        Requirements.Add(new MPCost(15));
        Effects.Add(new DamageEffect(45));
    }
}