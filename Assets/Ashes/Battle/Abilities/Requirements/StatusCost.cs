public class StatusCost : AbilityRequirement
{
    public string StatusId { get; }
    public bool RequirePresence { get; }

    public StatusCost(string statusId, bool requirePresence)
    {
        StatusId = statusId;
        RequirePresence = requirePresence;
    }

    public override bool MeetsRequirement(ActorId actorId, BattleContext context)
    {
        var actor = context.Actors.GetActor(actorId);
        if (actor == null)
        {
            return false;
        }

        bool hasStatus = actor.ActiveStatuses.Exists(s => s.StatusId == StatusId);

        // If RequirePresence is true, we MUST HAVE the status (e.g. Dragoog requires "status_airborne").
        // if RequirePresence is false, we MUST NOT HAVE the status (e.g.  Mage requires NO "status_silence").
        return RequirePresence ? hasStatus : !hasStatus;
    }

    public override void ConsumeRequirement(AbilityContext context)
    {
        // Do nothing
        // Although may decide to have an ability consume a status in future...


        // TODO: Should this match other Cost types?
        // Publish Event??? Need ResourceType.Status???
        // context.Events.Publish(new ResourceConsumedEvent(caster.Id, ResourceType.Status, cost));
    }
}