using UnityEngine;
using UnityEngine.AI;

public class ActorView : MonoBehaviour
{
    private ActorId actorId;
    private NavMeshObstacle navMeshObstacle;

    public void Initialize(BattleEventBus eventBus, ActorId actorId, SimVector3 initialPosition, float actorRadius = 1.0f)
    {
        this.actorId = actorId;
        transform.position = VectorAdapter.ToUnity(initialPosition);

        navMeshObstacle = GetComponent<NavMeshObstacle>();
        navMeshObstacle.radius = actorRadius;
        navMeshObstacle.carving = true;

        eventBus.Subscribe<ActorMovedEvent>(OnActorMoved);
        // TODO: Subscribe to other events for floating text
    }

    private void OnActorMoved(ActorMovedEvent e)
    {
        if (e.ActorId.Value == actorId.Value)
        {
            transform.position = VectorAdapter.ToUnity(e.Position);
        }
    }
}