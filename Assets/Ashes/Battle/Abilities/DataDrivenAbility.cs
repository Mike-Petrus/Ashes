public class DataDrivenAbility : Ability
{
    // In battle, when executing an ability, get the template from the
    // database and instantiate a DD-Ability. This is then used to execute the ability
    public DataDrivenAbility(AbilityTemplate template)
    {
        // Identity
        AbilityId = template.AbilityId;
        Name = template.Name;

        // Core Properties
        Category = template.Category;
        ImpactType = template.Impact;
        ElementType = template.Element;

        // Spatial Rules
        Range = template.Range;
        Radius = template.Radius;
        Angle = template.Angle;
        RequiresLoS = template.RequiresLoS;

        // Targeting Rules
        Mode = template.Mode;
        Alignment = template.Alignment;
        CanTargetDead = template.CanTargetDead;
        RefundPercent = template.RefundPercent;

        Requirements.AddRange(template.Requirements);
        Effects.AddRange(template.Effects);
    }
}