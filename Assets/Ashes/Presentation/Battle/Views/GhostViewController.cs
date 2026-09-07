using UnityEngine;

public class GhostViewController : MonoBehaviour
{
    [Header("Ghost Settings")]
    [Tooltip("The child GameObject that contains the 3D model/cube")]
    public GameObject VisualRoot; 
    
    [Tooltip("The translucent material to apply to the clone")]
    public Material GhostMaterial;

    [Header("Temporary Fixes")]
    [Tooltip("Offset for primitive shapes with center-pivots. Set to 0 when using real 3D models with bottom-pivots")]
    public float YOffset = 0.5f;

    private GameObject ghostInstance;

    public void Initialize()
    {
        if (VisualRoot == null || GhostMaterial == null)
        {
            Debug.LogWarning($"[GhostPreview] Missing VisualRoot or Material on {gameObject.name}");
            return;
        }

        // 1. Clone the visual root
        ghostInstance = Instantiate(VisualRoot, transform);
        ghostInstance.name = $"{VisualRoot.name}_GhostPreview";

        // 2. Sanitize the clone (Strip away logic, physics, and colliders)
        SanitizeClone(ghostInstance);

        // 3. Swap the materials
        ApplyGhostMaterial(ghostInstance);

        // 4. Hide by default
        ghostInstance.SetActive(false);
    }

    private void SanitizeClone(GameObject clone)
    {
        // Get EVERY component on the clone and its children
        Component[] allComponents = clone.GetComponentsInChildren<Component>(true);

        foreach (var comp in allComponents)
        {
            // KEEP these components so the mesh still renders
            if (comp is Transform || 
                comp is MeshFilter || 
                comp is MeshRenderer || 
                comp is SkinnedMeshRenderer)
            {
                continue;
            }

            // DESTROY everything else (Colliders, Rigidbodies, custom scripts, NavMeshObstacles)
            // Note: If you want ghosts to play Idle animations later, add "comp is Animator" to the list
            Destroy(comp);
        }
    }

    private void ApplyGhostMaterial(GameObject clone)
    {
        Renderer[] renderers = clone.GetComponentsInChildren<Renderer>(true);

        foreach (var rend in renderers)
        {
            // Create a new array matching the length of the original materials
            Material[] ghostMats = new Material[rend.sharedMaterials.Length];
            
            // Fill every material slot with the ghost material
            for (int i = 0; i < ghostMats.Length; i++)
            {
                ghostMats[i] = GhostMaterial;
            }

            rend.sharedMaterials = ghostMats;
        }
    }

    // Called by the Event Bus / View Manager later
    public void SetGhostState(bool isVisible, Vector3 worldPosition = default)
    {
        if (ghostInstance == null) return;

        if (isVisible)
        {
            ghostInstance.SetActive(true);
            
            // Because the ghost is a child of the ActorView, we must set its WORLD position explicitly 
            // so it detaches from the parent's current physical location.
            // TEMP: Add the YOffset for placeholder models that clip the ground
            ghostInstance.transform.position = worldPosition + new Vector3(0, YOffset, 0);
        }
        else
        {
            ghostInstance.SetActive(false);
        }
    }
}