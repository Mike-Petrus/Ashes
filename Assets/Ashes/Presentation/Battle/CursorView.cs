using UnityEngine;

public class CursorView : MonoBehaviour
{
    [Header("Visuals")]
    public GameObject CursorVisuals;
    public Renderer CursorRenderer;
    public LayerMask GroundLayer;
    public float RaycastHeight = 50f;

    [Header("Line Renderers")]
    public LineRenderer pathLineRenderer;
    public LineRenderer aoeLineRenderer;

    [Header("Colors")]
    public Color ValidColor = Color.white;
    public Color InvalidColor = Color.red;

    public void Initialize(BattleEventBus eventBus)
    {
        if (pathLineRenderer != null)
        {
            pathLineRenderer.positionCount = 0;
        }
        if (aoeLineRenderer != null)
        {
            aoeLineRenderer.positionCount = 0;
        }

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
            if (aoeLineRenderer != null)
            {
                aoeLineRenderer.positionCount = 0;
            }

            return;
        }

        CursorVisuals.SetActive(true);
        Color currentColor = e.IsValid ? ValidColor : InvalidColor;

        // 1. Create a ray starting high above the simulated position
        Vector3 origin = VectorAdapter.ToUnity(e.Position);
        origin.y += RaycastHeight;

        Ray ray = new Ray(origin, Vector3.down);
        Vector3 finalPosition = VectorAdapter.ToUnity(e.Position);

        // 2. Perform the raycast
        if (Physics.Raycast(ray, out RaycastHit hit, RaycastHeight * 2f, GroundLayer))
        {
            finalPosition = hit.point;
            transform.position = finalPosition + (hit.normal * 0.02f);
            transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }
        else
        {   
            // If we are off the map
            transform.position = finalPosition;
            transform.rotation = Quaternion.identity;
        }

        if (CursorRenderer != null)
        {
            CursorRenderer.material.color = currentColor;
        }

        // 3. Draw the Movement Path
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

        // 4. Draw the AoE Shape
        if (aoeLineRenderer != null)
        {
            bool isAreaEffect = (e.Mode == TargetingMode.PointAoE || e.Mode == TargetingMode.HybridAoE || e.Mode == TargetingMode.ActorAoE);

            if (e.Radius > 0 && isAreaEffect)
            {
                DrawAoECircle(finalPosition, e.Radius, currentColor);
            }
            else
            {
                aoeLineRenderer.positionCount = 0;
            }
        }
    }

    private void DrawAoECircle(Vector3 center, float radius, Color color)
    {
        int segments = 36;  // 1 point / 10 degrees
        aoeLineRenderer.positionCount = segments + 1;

        aoeLineRenderer.startColor = color;
        aoeLineRenderer.endColor = color;

        float angleStep = 360f / segments;

        for (int i = 0; i <= segments; i++)
        {
            // convert to rad
            float rad = Mathf.Deg2Rad * (i * angleStep);

            // Calculate X and Z offsets based on the radius
            Vector3 offset = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * radius;

            Vector3 point = center + offset;

            // Raise slightly above ground
            point.y += 0.05f;

            aoeLineRenderer.SetPosition(i, point);
        }
    }
}