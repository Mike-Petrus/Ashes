using UnityEngine;
using UnityEngine.AI;

public class ActorView : MonoBehaviour
{
    private ActorId actorId;
    private NavMeshObstacle navMeshObstacle;
    private Outline outlineComponent;

    public ActorId ActorId => actorId;

    public void Initialize(BattleEventBus eventBus, ActorId actorId, SimVector3 initialPosition, float actorRadius = 1.0f)
    {
        this.actorId = actorId;
        transform.position = VectorAdapter.ToUnity(initialPosition);

        navMeshObstacle = GetComponent<NavMeshObstacle>();
        navMeshObstacle.radius = actorRadius;
        navMeshObstacle.carving = true;

        // Find the Outline script on the child Cube
        outlineComponent = GetComponentInChildren<Outline>();
        if (outlineComponent != null)
        {
            // Per instructions: configure mode and disable initially
            outlineComponent.OutlineMode = Outline.Mode.OutlineAll;
            outlineComponent.enabled = false;
        }
        else
        {
            Debug.LogWarning($"[ActorView] No Outline component found on children of {gameObject.name}!");
        }

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

    public void ApplyHighlight(Color highlightColor, float width = 5f)
    {
        if (outlineComponent == null) return;

        // Optimization: Only write to the component if properties actually changed
        // This prevents the Outline script from needlessly recalculating meshes every frame
        if (!outlineComponent.enabled || outlineComponent.OutlineColor != highlightColor || outlineComponent.OutlineWidth != width)
        {
            outlineComponent.OutlineColor = highlightColor;
            outlineComponent.OutlineWidth = width;
            outlineComponent.enabled = true;
        }
    }

    public void ClearHighlight()
    {
        if (outlineComponent != null && outlineComponent.enabled)
        {
            outlineComponent.enabled = false;
        }
    }

    private void OnDestroy()
    {
        // Add unsubscription later when we have a cached event bus
    }
}