using System.Collections.Generic;

public class AbilitySet
{
    public Dictionary<string, List<Ability>> AvailableAbilities { get; private set; }

    public void UnlockAbility(Ability ability)
    {
        if (!AvailableAbilities.ContainsKey(ability.Category))
        {
            AvailableAbilities[ability.Category] = new List<Ability>();
        }

        AvailableAbilities[ability.Category].Add(ability);
    }
}