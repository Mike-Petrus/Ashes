using System.Collections.Generic;

public class ClassTemplate
{
    public string ClassName;
    public CoreAttributes BaseStats;
    public List<AbilityUnlock> LearnedAbilities = new();

    // public GrowthRates - may want to add individual growth rates later
}