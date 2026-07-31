// A pure data representation of a modifier, separate from the runtime tracking

public class StatModifierTemplate
{
    public StatType Stat { get; set; }
    public float FlatValue { get; set; }
    public float PercentValue { get; set; }
}