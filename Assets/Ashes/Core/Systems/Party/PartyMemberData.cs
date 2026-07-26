using System.Collections.Generic;

public class PartyMemberData
{
    public string CharacterId { get; private set; }
    public string CharacterName { get; private set; }
    public ClassTemplate CharacterClass { get; private set; }

    // Core stats that grow with level up
    public CharacterStats BaseStats { get; private set; }
    
    public int CurrentLevel { get; set; }

    // Persistant vitals
    public int CurrentHP { get; set; }
    public int CurrentMP { get; set; }

    public bool IsAlive => CurrentHP > 0;

    // TODO: Track equipped Abilities/Classes and Equipment
    public List<string> UnlockedAbilities { get; private set; } = new List<string>();

    public PartyMemberData(string id, string name, ClassTemplate charClass, CharacterStats stats, int level)
    {
        CharacterId = id;
        CharacterName = name;
        CharacterClass = charClass;
        BaseStats = stats;

        CurrentLevel = level;
        CurrentHP = stats.MaxHP;
        CurrentMP = stats.MaxMP;

        UpdateUnlockedAbilities();
    }

    public void UpdateUnlockedAbilities()
    {
        UnlockedAbilities.Clear();

        if (CharacterClass == null)
        {
            return;
        }

        foreach (var ability in CharacterClass.LearnedAbilities)
        {
            if (CurrentLevel >= ability.RequiredLevel)
            {
                UnlockedAbilities.Add(ability.AbilityId);
            }
        }
    }
}