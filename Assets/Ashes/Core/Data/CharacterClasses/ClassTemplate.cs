using System.Collections.Generic;

public class ClassTemplate
{
    public string ClassId { get; set; }
    public string ClassName { get; set; }

    public CoreAttributes BaseStats { get; set; }
    
    public List<AbilityUnlock> LearnedAbilities { get; set; } = new();

    // public GrowthRates - may want to add individual growth rates later
}