public class UseItemAbility : Ability
{
    public UseItemAbility(Item item)
    {
        Name = $"Use {item.Name}";
        Category = "Item";
        Range = item.Range;
        Radius = item.Radius;
        Mode = item.TargetMode;
        Alignment = item.Alignment;
        RefundPercent = 0.50f;

        Requirements.Add(new ItemCost(item.Id, 1));
        Effects.AddRange(item.Effects);
    }
}