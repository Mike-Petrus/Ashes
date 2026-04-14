using System.Collections.Generic;
using System.Linq;

public class MovementSystem : IBattleSystem
{
    private BattleEventBus events;
    private IPathfinder pathfinder;
    private ActorStateSystem actorStates;
    private ActorRegistry actors;

    private Dictionary<ActorId, MovementState> activeMoves = new();

    // TODO: After NavMesh path is generated, introduce MovementPath
    // ex. List<SimVector3> waypoints
    // Update will then move between waypoints instead of a straight line Start -> Destination

    private const float MoveDuration = 1.0f; // Global battle speed

    public MovementSystem(BattleEventBus eventBus, IPathfinder paths, ActorStateSystem states, ActorRegistry actorRegistry)
    {
        events = eventBus;
        pathfinder = paths;
        actorStates = states;
        actors = actorRegistry;

        events.Subscribe<MoveRequestEvent>(OnMoveRequest);
    }

    private void OnMoveRequest(MoveRequestEvent e)
    {
        var actor = actors.GetActor(e.ActorId);
        var path = pathfinder.FindPath(e.Start, e.Destination, actor.Radius);

        if (path == null || path.Count < 2)
        {
            path = new List<SimVector3> {e.Start, e.Destination };
        }

        var moveState = new MovementState
        {
            ActorId = e.ActorId,
            Waypoints = path,
            CurrentIndex = 0,
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

            SimVector3 startNode = moveState.Waypoints[moveState.CurrentIndex];
            SimVector3 endNode = moveState.Waypoints[moveState.CurrentIndex + 1];

            float segmentDistance = SimVector3.Distance(startNode, endNode);
            if (segmentDistance == 0)
            {
                segmentDistance = 0.01f;
            }

            float moveSpeed = 1f;
            moveState.Progress += (deltaTime * moveSpeed) / segmentDistance;

            if (moveState.Progress >= 1f)
            {
                moveState.Progress = 0f;
                moveState.CurrentIndex++;

                if (moveState.CurrentIndex >= moveState.Waypoints.Count - 1)
                {
                    var finalPos = moveState.Waypoints.Last();
                    actors.GetActor(actorId).Position = finalPos;
                    events.Publish(new ActorMovedEvent(actorId, finalPos));

                    activeMoves.Remove(actorId);
                    actorStates.SetState(actorId, ActorState.Idle);
                    events.Publish(new MoveCompletedEvent(actorId));
                    continue;
                }

                // Update start/end nodes for next segment calculation
                startNode = moveState.Waypoints[moveState.CurrentIndex];
                endNode = moveState.Waypoints[moveState.CurrentIndex + 1];
            }

            // Lerp between the current node and next node
            SimVector3 position = startNode + (endNode - startNode) * moveState.Progress;
            actors.GetActor(actorId).Position = position;
            events.Publish(new ActorMovedEvent(actorId, position));
        }
    }
}