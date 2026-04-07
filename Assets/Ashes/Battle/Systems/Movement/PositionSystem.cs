using System.Collections.Generic;

public class PositionSystem
{
    private ActorRegistry actors;
    private Dictionary<ActorId, SimVector3> reservedSpaces = new();

    public PositionSystem(ActorRegistry actorRegistry)
    {
        actors = actorRegistry;
    }

    public void Update(float deltaTime)
    {
        // May not need per-frame updates
        // primarily responds to queueries and events
        // If needed later, inherit from IBattleSystem
    }

    public void ReserveSpace(ActorId actorId, SimVector3 destination)
    {
        reservedSpaces[actorId] = destination;
    }

    public void ClearReservation(ActorId actorId)
    {
        reservedSpaces.Remove(actorId);
    }

    public bool IsSpaceOccupied(SimVector3 targetPosition, float targetRadius, ActorId movingActor)
    {
        foreach (var actor in actors.GetAllActors())
        {
            if (actor.Id.Equals(movingActor))
            {
                continue;
            }

            float distance = SimVector3.Distance(targetPosition, actor.Position);

            if (distance < (targetRadius + actor.Radius))
            {
                return true;
            }     
        }

        foreach (var kvp in reservedSpaces)
        {
            ActorId id = kvp.Key;
            SimVector3 pos = kvp.Value;

            if (id.Equals(movingActor))
            {
                continue;
            }

            float distance = SimVector3.Distance(targetPosition, pos);

            if (distance < (targetRadius + actors.GetActor(id).Radius))
            {
                return true;
            }
        }

        return false;
    }

    // For Phase 3B: AoE Targeting
    public List<ActorId> GetActorsInRadius(SimVector3 center, float radius)
    {
        List<ActorId> targets = new();

        foreach (var actor in actors.GetAllActors())
        {
            float distance = SimVector3.Distance(center, actor.Position);

            if (distance <= (radius + actor.Radius))
            {
                targets.Add(actor.Id);
            }
        }

        return targets;
    }
}