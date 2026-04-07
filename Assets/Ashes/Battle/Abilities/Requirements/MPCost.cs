public class MPCost : AbilityRequirement
{
    private int cost;

    public MPCost(int mpCost)
    {
        cost = mpCost;
    }

    public override bool MeetsRequirement(ActorId casterId, ActorRegistry actors)
    {
        var caster = actors.GetActor(casterId);
        return caster != null && caster.CurrentMP >= cost;
    }

    public override void ConsumeRequirement(AbilityContext context)
    {
        var caster = context.Actors.GetActor(context.SourceId);
        caster.CurrentMP -= cost;

        context.Events.Publish(new ResourceConsumedEvent(caster.Id, ResourceType.MP, cost));
    }
}