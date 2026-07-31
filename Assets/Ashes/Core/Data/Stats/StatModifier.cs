public class StatModifier
{
    public float FlatValue;
    public float PercentValue;
    public object Source; // Stores the ActiveStatus or Equipment applying this

    public StatModifier(float flatValue, float percentValue, object source)
    {
        FlatValue = flatValue;
        PercentValue = percentValue;
        Source = source;
    }
}