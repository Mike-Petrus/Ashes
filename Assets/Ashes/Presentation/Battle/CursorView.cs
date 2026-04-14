using UnityEngine;

public class CursorView : MonoBehaviour
{
    [Header("Visuals")]
    public GameObject CursorVisuals;
    public Renderer CursorRenderer;
    public LayerMask GroundLayer;
    public float RaycastHeight = 50f;

    private LineRenderer pathLineRenderer;

    [Header("Colors")]
    public Color ValidColor = Color.white;
    public Color InvalidColor = Color.red;

    public void Initialize(BattleEventBus eventBus)
    {
        pathLineRenderer = GetComponent<LineRenderer>();

        CursorVisuals.SetActive(false);
        
        eventBus.Subscribe<CursorMovedEvent>(OnCursorMoved);
    }

    private void OnCursorMoved(CursorMovedEvent e)
    {
        if (!e.IsVisible)
        {
            CursorVisuals.SetActive(false);

            if (pathLineRenderer != null)
            {
                pathLineRenderer.positionCount = 0;
            }

            return;
        }

        CursorVisuals.SetActive(true);

        // 1. Create a ray starting high above the simulated position
        Vector3 origin = VectorAdapter.ToUnity(e.Position);
        origin.y += RaycastHeight;

        Ray ray = new Ray(origin, Vector3.down);

        // 2. Perform the raycast
        if (Physics.Raycast(ray, out RaycastHit hit, RaycastHeight * 2f, GroundLayer))
        {
            transform.position = hit.point + (hit.normal * 0.02f);

            transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }
        else
        {   
            // If we are off the map
            transform.position = VectorAdapter.ToUnity(e.Position);
            transform.rotation = Quaternion.identity;
        }

        if (CursorRenderer != null)
        {
            CursorRenderer.material.color = e.IsValid ? ValidColor : InvalidColor;
        }

        if (pathLineRenderer != null)
        {
            if (e.Path != null && e.Path.Count > 0)
            {
                pathLineRenderer.positionCount = e.Path.Count;
                
                for (int i = 0; i < e.Path.Count; i++)
                {
                    Vector3 point = VectorAdapter.ToUnity(e.Path[i]);
                    point.y += 0.1f;
                    pathLineRenderer.SetPosition(i, point);
                }

                pathLineRenderer.startColor = e.IsValid ? ValidColor : InvalidColor;
                pathLineRenderer.endColor = e.IsValid ? ValidColor : InvalidColor;
            }
            else
            {
                pathLineRenderer.positionCount = 0;
            }
        }
    }
}