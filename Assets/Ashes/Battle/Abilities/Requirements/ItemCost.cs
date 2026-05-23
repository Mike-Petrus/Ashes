public class ItemCost : AbilityRequirement
{
    private string itemId;
    private int amount;

    public ItemCost(string itemId, int amount = 1)
    {
        this.itemId = itemId;
        this.amount = amount;
    }

    public override bool MeetsRequirement(ActorId casterId, BattleContext context)
    {
        // Validation: Do we have enough of this item in the inventory?
        return context.Inventory.HasItem(itemId, amount);
    }

    public override void ConsumeRequirement(AbilityContext context)
    {
        var caster = context.Actors.GetActor(context.SourceId);
        context.Inventory.ConsumeItem(itemId, amount);

        context.Events.Publish(new ResourceConsumedEvent(caster.Id, ResourceType.Item, amount));
    }
}