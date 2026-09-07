using UnityEngine;

// This visual requires a Cylinder Mesh Filter and Mesh Renderer
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ArenaBoundaryView : MonoBehaviour
{
    private BattleEventBus events;

    // We leave the visual logic completely in the shader graph.
    // The C# script just physically places the mesh where it belongs.

    public void Initialize(BattleEventBus eventBus)
    {
        events = eventBus;
        
        // Hide by default
        gameObject.SetActive(false);

        events.Subscribe<ArenaInitializedEvent>(OnArenaInitialized);
    }

    private void OnArenaInitialized(ArenaInitializedEvent e)
    {
        gameObject.SetActive(true);

        // 1. Position the Cylinder at the Simulation Center
        transform.position = VectorAdapter.ToUnity(e.Center);

        // 2. Physical Scale (The Professional Solution)
        // Cylinder scale must account for both Diameter (X/Z) and Height (Y).
        // Standard Cylinder is 2m tall. Let's make it 1m tall so it doesn't tower over actors.
        float desiredHeight = 1.0f;
        transform.localScale = new Vector3(e.Radius * 2, desiredHeight / 2.0f, e.Radius * 2);
    }

    private void OnDestroy()
    {
        if (events != null) events.Unsubscribe<ArenaInitializedEvent>(OnArenaInitialized);
    }
}