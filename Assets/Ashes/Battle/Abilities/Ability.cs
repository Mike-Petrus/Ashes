public abstract class Ability
{
    public string Name;
    public float Range;

    public abstract void Execute(AbilityContext context);
}