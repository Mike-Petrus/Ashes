using System.Collections.Generic;

public class PositionSystem
{
    private BattleEventBus events;
    private ActorRegistry actors;
    private Dictionary<ActorId, SimVector3> reservedSpaces = new();

    public PositionSystem(ActorRegistry actorRegistry, BattleEventBus eventBus)
    {
        actors = actorRegistry;
        events = eventBus;

        events.Subscribe<MoveCompletedEvent>(OnMoveCompleted);
        events.Subscribe<CommandFinishedEvent>(OnCommandFinished);
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

    private void OnMoveCompleted(MoveCompletedEvent e)
    {
        // The actor has physically arrived. Their physical body now blocks the space, 
        // so we drop the invisible future reservation.
        ClearReservation(e.ActorId);
    }

    private void OnCommandFinished(CommandFinishedEvent e)
    {
        // FAILSAFE: If a command finishes, aborts, or fizzles, wipe the actor's reservation.
        // If the move was successful, it was already cleared by OnMoveCompleted.
        if (e.Command != null)
        {
            ClearReservation(e.Command.ActorId);
        }
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

    // For Phase 3B: AoE Targeting --- Deprecated ???
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