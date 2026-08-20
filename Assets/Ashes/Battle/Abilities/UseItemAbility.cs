public class UseItemAbility : Ability
{
    public UseItemAbility(ItemTemplate item)
    {
        Name = $"Use {item.Name}";
        Category = "Item";
        ImpactType = item.Impact;
        ElementType = item.Element;

        // Spatial Rules
        Range = item.Range;
        Radius = item.Radius;
        Angle = item.Angle;
        RequiresLoS = item.RequiresLoS;
        
        // Targeting Rules
        Mode = item.TargetMode;
        Alignment = item.Alignment;
        CanTargetDead = item.CanTargetDead;
        RefundPercent = 0.50f;

        Requirements.Add(new ItemCost(item.ItemId, 1));
        Effects.AddRange(item.Effects);
    }
}