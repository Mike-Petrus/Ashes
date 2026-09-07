public class MenuOptionHoveredEvent : IBattleEvent
{
    public string AbilityId { get; }
    public string Name { get; }
    public string Category { get; }

    public MenuOptionHoveredEvent(string abilityId, string name, string category)
    {
        AbilityId = abilityId;
        Name = name;
        Category = category;
    }
}