public class ResourceConsumedEvent : IBattleEvent
{
    public ActorId ActorId;
    public ResourceType Resource;
    public int Amount;

    public ResourceConsumedEvent(ActorId actorId, ResourceType resourceType, int amount)
    {
        ActorId = actorId;
        Resource = resourceType;
        Amount = amount;
    }
}