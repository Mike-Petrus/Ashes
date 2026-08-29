using System.Collections.Generic;
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
        ClearVisuals();        
        eventBus.Subscribe<CursorMovedEvent>(OnCursorMoved);
    }

    private void OnCursorMoved(CursorMovedEvent e)
    {
        if (!e.IsVisible)
        {
            ClearVisuals();
            return;
        }

        CursorVisuals.SetActive(true);
        Color currentColor = e.IsValid ? ValidColor : InvalidColor;

        // Resolve physical ground position for the cursor
        Vector3 targetPos = GetGroundPosition(VectorAdapter.ToUnity(e.Position), out Vector3 targetNormal);

        UpdateCursorMarker(targetPos, targetNormal, currentColor);
        UpdatePathVisuals(e.Path, currentColor);
        UpdateAoEVisuals(e, targetPos, currentColor);
    }

    private void ClearVisuals()
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
    }

    private Vector3 GetGroundPosition(Vector3 simPos, out Vector3 normal)
    {
        Vector3 origin = simPos;
        origin.y += RaycastHeight;
        Ray ray = new Ray(origin, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, RaycastHeight * 2f, GroundLayer))
        {
            normal = hit.normal;
            return hit.point;
        }

        // Fallback if off the map
        normal = Vector3.up;
        return simPos;
    }

    private void UpdateCursorMarker(Vector3 position, Vector3 normal, Color color)
    {
        transform.position = position + (normal * 0.02f);
        transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);

        if (CursorRenderer != null)
        {
            CursorRenderer.material.color = color;
        }
    }

    private void UpdatePathVisuals(List<SimVector3> path, Color color)
    {
        if (pathLineRenderer == null)
        {
            return;
        }

        if (path == null || path.Count == 0)
        {
            return;
        }

        pathLineRenderer.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
        {
            // Raycast the path points to ground them properly
            Vector3 point = GetGroundPosition(VectorAdapter.ToUnity(path[i]), out _);
            point.y += 0.1f;
            pathLineRenderer.SetPosition(i, point);
        }

        pathLineRenderer.startColor = color;
        pathLineRenderer.endColor = color;
    }

    private void UpdateAoEVisuals(CursorMovedEvent e, Vector3 targetPos, Color color)
    {
        if (aoeLineRenderer == null)
        {
            return;
        }

        if (e.Radius <= 0)
        {
            aoeLineRenderer.positionCount = 0;
            return;
        }

        switch(e.Mode)
        {
            case TargetingMode.PointAoE:
            case TargetingMode.ActorAoE:
            case TargetingMode.HybridAoE:
                DrawAoECircle(targetPos, e.Radius, color);
                break;

            case TargetingMode.Directional:
                Vector3 centerPos = e.StaticCenter.HasValue ? GetGroundPosition(VectorAdapter.ToUnity(e.StaticCenter.Value), out _) : targetPos;
                DrawAoECone(targetPos, centerPos, e.Radius, e.Angle, color);
                break;

            default:
                aoeLineRenderer.positionCount = 0;
                break;
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
            float rad = Mathf.Deg2Rad * (i * angleStep);
            Vector3 offset = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * radius;
            Vector3 point = center + offset;

            // Raise slightly above ground
            point.y += 0.05f;
            aoeLineRenderer.SetPosition(i, point);
        }
    }

    private void DrawAoECone(Vector3 cursorPos, Vector3 staticCenter, float radius, float angle, Color color)
    {
        // TODO: Turn off CursorVisuals. We only want to see the cone, not the cursor position
        // For now, keep for debugging
        Vector3 forwardDir = Vector3.forward;

        if (Vector3.Distance(staticCenter, cursorPos) > 0.01f)
        {
            forwardDir = cursorPos - staticCenter;
            forwardDir.y = 0;
            forwardDir.Normalize();
        }

        int segments = 20;

        aoeLineRenderer.positionCount = segments + 2; // Need points for Origin, Arc Segments, and back to Origin
        aoeLineRenderer.startColor = color;
        aoeLineRenderer.endColor = color;

        // Calculate the starting angle (half the total angle)
        float startAngle = -angle / 2f;
        float angleStep = angle / segments;

        Vector3 raisedCenter = staticCenter + new Vector3(0, 0.05f, 0);
        aoeLineRenderer.SetPosition(0, raisedCenter);

        // Draw the Arc
        for (int i = 0; i <= segments; i++)
        {
           float currentAngle = startAngle + (i * angleStep);

            // Rotate the forward vector by the current angle around the Y axis
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
            Vector3 direction = rotation * forwardDir;

            Vector3 point = staticCenter + (direction * radius);
            point.y += 0.05f;
            aoeLineRenderer.SetPosition(i + 1, point);
        }
        aoeLineRenderer.loop = true; 
    }
}