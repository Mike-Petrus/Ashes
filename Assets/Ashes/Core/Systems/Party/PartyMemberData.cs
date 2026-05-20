public class PartyMemberData
{
    public string CharacterId { get; private set; }
    public string CharacterName { get; private set; }

    // Core stats that grow with level up
    public CharacterStats BaseStats { get; private set; }

    // Persistant vitals
    public int CurrentHP { get; set; }
    public int CurrentMP { get; set; }

    public bool IsAlive => CurrentHP > 0;

    // TODO: Track equipped Abilities/Classes and Equipment

    public PartyMemberData(string id, string name, CharacterStats stats)
    {
        CharacterId = id;
        CharacterName = name;
        BaseStats = stats;

        CurrentHP = stats.MaxHP;
        CurrentMP = stats.MaxMP;
    }
}