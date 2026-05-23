public abstract class AbilityRequirement
{
    // Evaluated by the UI and CommandValidator
    public abstract bool MeetsRequirement(ActorId casterId, BattleContext context);

    // Executed by the Abilty when it successfully fires
    public abstract void ConsumeRequirement(AbilityContext context);
}