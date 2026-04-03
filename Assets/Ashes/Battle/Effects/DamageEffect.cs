public class DamageEffect : Effect
{
    public int BaseDamage { get; }
    // public ElementType Element { get; }  TODO: implement elemental enum

    public DamageEffect(int baseDamage)
    {
        BaseDamage = baseDamage;
    }
}