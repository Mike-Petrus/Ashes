public class DataDrivenAbility : Ability
{
    // In battle, when executing an ability, get the template from the
    // database and instantiate a DD-Ability. This is then used to execute the ability
    public DataDrivenAbility(AbilityTemplate template)
    {
        Name = template.Name;
        Category = template.Category;
        Range = template.Range;
        Radius = template.Radius;
        Angle = template.Angle;
        RequiresLoS = template.RequiresLoS;
        Mode = template.Mode;
        Alignment = template.Alignment;
        RefundPercent = template.RefundPercent;

        Requirements.AddRange(template.Requirements);
        Effects.AddRange(template.Effects);
    }
}