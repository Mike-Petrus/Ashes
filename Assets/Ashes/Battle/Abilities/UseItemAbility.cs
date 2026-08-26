public class UseItemAbility : Ability
{
    public UseItemAbility(ItemTemplate item)
    {
        AbilityId = $"ability_item_{item.ItemId}";          // TODO: I have actually no idea how the fuck this will work if you need to query the database"
        Name = $"Use {item.Name}";                          // Probably needs just 1 ID, but IDK. We should never need to look up the description for this anyways
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