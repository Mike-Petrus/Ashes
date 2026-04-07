using System.Collections.Generic;

public class PoisonDartAbility : Ability
{
    public PoisonDartAbility()
    {
        Name = "Poison Dart";
        Category = "Monster Ability";
        Range = 20f;
        Mode = TargetingMode.SingleTarget;
        Alignment = TargetAlignment.Enemy;
        
        // 2. The Poison ticks every 3 seconds, doing 2 damage per tick, lasting for 15 seconds!
        var poisonTickPayload = new List<Effect> {new DamageEffect(2) };
        Effects.Add(new DamageEffect(5));
        Effects.Add(new ApplyStatusEffect("Poison", 15f, 3f, poisonTickPayload));

    }
}