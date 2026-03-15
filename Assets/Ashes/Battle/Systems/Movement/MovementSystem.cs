using System.Collections.Generic;
using System.Linq;

public class MovementSystem : IBattleSystem
{
    private EventBus events;
    private ActorStateSystem actorStates;
    private ActorRegistry actors;

    // TODO: Change to Dictionary<Id, MovementState>
    // Will need to refactor other methods to get Actor for ActorRegistry using Id
    private Dictionary<ActorId, MovementState> activeMoves = new();

    // TODO: After NavMesh path is generated, introduce MovementPath
    // ex. List<SimVector3> waypoints
    // Update will then move between waypoints instead of a straight line Start -> Destination

    private const float MoveDuration = 1.0f; // Global battle speed

    public MovementSystem(EventBus eventBus, ActorStateSystem states, ActorRegistry actorRegistry)
    {
        events = eventBus;
        actorStates = states;
        actors = actorRegistry;

        events.Subscribe<MoveRequestEvent>(OnMoveRequest);
    }

    private void OnMoveRequest(MoveRequestEvent e)
    {
        var moveState = new MovementState
        {
            ActorId = e.ActorId,
            Start = e.Start,
            Destination = e.Destination,
            Progress = 0f
        };

        activeMoves[e.ActorId] = moveState;

        actorStates.SetState(e.ActorId, ActorState.Moving);
    }

    public void Update(float deltaTime)
    {
        foreach (var pair in activeMoves.ToList())
        {
            var actorId = pair.Key;
            var moveState = pair.Value;

            moveState.Progress += deltaTime / MoveDuration;

            float t = moveState.Progress;

            if (t > 1f)
            {
                t = 1f;
            }

            SimVector3 position = moveState.Start + (moveState.Destination - moveState.Start) * t;

            var actor = actors.GetActor(actorId);
            actor.Position = position;

            events.Publish(new ActorMovedEvent(actorId, position));

            if (moveState.Progress >= 1f)
            {
                activeMoves.Remove(actorId);

                actorStates.SetState(actorId, ActorState.Idle);

                events.Publish(new MoveCompletedEvent(actorId));
            }
        }
    }
}