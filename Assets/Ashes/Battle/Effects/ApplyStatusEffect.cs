public class ApplyStatusEffect : Effect
{
    public string StatusId { get; }
    public float OverrideDuration { get; }
    
    // We pass the Power of the spell/item here so the pipeline 
    // knows how strong to make the Poison or Regen ticks!
    public int Power { get; } 

    public ApplyStatusEffect(string statusId, float overrideDuration, int power)
    {
        StatusId = statusId;
        OverrideDuration = overrideDuration;
        Power = power;
    }
}