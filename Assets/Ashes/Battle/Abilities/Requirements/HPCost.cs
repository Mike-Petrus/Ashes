public class HPCost : AbilityRequirement
{
    private int cost;

    public HPCost(int hpCost)
    {
        cost = hpCost;
    }

    public override bool MeetsRequirement(ActorId casterId, BattleContext context)
    {
        var caster = context.Actors.GetActor(casterId);
        return caster != null && caster.CurrentHP >= cost;
    }

    public override void ConsumeRequirement(AbilityContext context)
    {
        var caster = context.Actors.GetActor(context.SourceId);
        caster.CurrentHP -= cost;

        context.Events.Publish(new ResourceConsumedEvent(caster.Id, ResourceType.HP, cost));

        // If Ability kills the caster
        if (caster.CurrentHP <= 0)
        {
            context.Events.Publish(new ActorDiedEvent(caster.Id));
        }
    }
}