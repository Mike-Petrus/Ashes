using System.Collections.Generic;

public class CleanseEffect : Effect
{
    public bool CleanseAllDebuffs { get; }
    public List<string> StatusIdsToCleanse { get; }

    public CleanseEffect(bool cleanseAllDebuffs, List<string> statusIdsToCleanse = null)
    {
        CleanseAllDebuffs = cleanseAllDebuffs;
        StatusIdsToCleanse = statusIdsToCleanse ?? new List<string>();
    }
}