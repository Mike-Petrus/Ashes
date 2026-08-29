using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CursorVFXController : MonoBehaviour
{
    [Header("NEW: URP Decal Projectors")]
    public DecalProjector CircleProjector;
    public DecalProjector ConeProjector;

    [Header("NEW: Decal Materials")]
    public Material ValidCircleMaterial;
    public Material InvalidCircleMaterial;
    public Material ValidConeMaterial;
    public Material InvalidConeMaterial;
    public float DecalDepth = 10f; // How far the projector shines downward

    [Header("RETAINED: Path Visuals")]
    public LineRenderer PathLineRenderer;
    public LayerMask GroundLayer;
    public float RaycastHeight = 50f;

    [Header("DEPRECATED / DEBUG: Legacy Visuals")]
    [Tooltip("Keep these assigned if you want to compare old visuals, or leave null to disable.")]
    public GameObject LegacyCursorVisuals;
    public Renderer LegacyCursorRenderer;
    public LineRenderer LegacyAoELineRenderer;
    public Color ValidColor = Color.white;
    public Color InvalidColor = Color.red;

    [Header("Turn On/Off Old Pipeline")]
    [Tooltip("Draws old line renderer cursor outlines when turned on")]
    public bool ShowLegacyCursor = false;

    private BattleSimulation simulation;

    public void Initialize(BattleSimulation sim)
    {
        simulation = sim;
        ClearAllVisuals();
        simulation.Events.Subscribe<CursorMovedEvent>(OnCursorMoved);
    }

    private void OnCursorMoved(CursorMovedEvent e)
    {
        if (!e.IsVisible)
        {
            ClearAllVisuals();
            return;
        }

        Color currentColor = e.IsValid ? ValidColor : InvalidColor;

        // -------- NEW URP Decal Pipeline ---------- //
        UpdateDecals(e);

        // -------- OLD Path Draw Pipeline ---------- //
        UpdatePathVisuals(e.Path, currentColor);

        // -------- DEPRECATED    Pipeline ---------- //
        // Resolve physical ground position for the legacy cursor and line renderers
        if (ShowLegacyCursor)
        {
            Vector3 targetPos = GetGroundPosition(VectorAdapter.ToUnity(e.Position), out Vector3 targetNormal);
            UpdateLegacyVisuals(e, targetPos, targetNormal, currentColor);
        }
    }

    private void ClearAllVisuals()
    {
        // New Decals
        if (CircleProjector != null) CircleProjector.gameObject.SetActive(false);
        if (ConeProjector != null) ConeProjector.gameObject.SetActive(false);

        // Path Draw
        if (PathLineRenderer != null) PathLineRenderer.positionCount = 0;

        // Deprecated / Debugging
        if (LegacyCursorVisuals != null) LegacyCursorVisuals.SetActive(false);
        if (LegacyAoELineRenderer != null) LegacyAoELineRenderer.positionCount = 0;
    }

    ////////////// URP DECAL PROJECTOR ///////////////
    private void UpdateDecals(CursorMovedEvent e)
    {
        if (e.Mode == TargetingMode.Directional)
        {
            if (CircleProjector != null) CircleProjector.gameObject.SetActive(false);

            if (ConeProjector != null)
            {
                ConeProjector.gameObject.SetActive(true);

                // 1. Determine origin and target direction
                Vector3 originPos = e.StaticCenter.HasValue ? VectorAdapter.ToUnity(e.StaticCenter.Value) : VectorAdapter.ToUnity(e.Position);
                Vector3 targetPos = VectorAdapter.ToUnity(e.Position);
                targetPos.y = originPos.y; // Keep rotation flat on the horizontal plane

                Vector3 dir = targetPos - originPos;
                if (dir.sqrMagnitude < 0.001f)
                {
                    dir = Vector3.forward;
                }
                else
                {
                    dir.Normalize();
                }

                // 2. POSITION: Dead center on the origin
                ConeProjector.transform.position = originPos;

                // 3. ROTATION: +Z points down into the floor, +Y points at the target
                ConeProjector.transform.rotation = Quaternion.LookRotation(Vector3.down, dir);

                // 4. SIZE: Because the tip is in the exact center of the image, 
                // the projector needs to be twice as long as the radius to fit it.
                float coneWidth = Mathf.Tan((e.Angle / 2f) * Mathf.Deg2Rad) * e.Radius * 2.0f;
                ConeProjector.size = new Vector3(coneWidth, e.Radius * 2f, DecalDepth);

                ConeProjector.material = e.IsValid ? ValidConeMaterial : InvalidConeMaterial;
            }
        }
        else
        {
            // Everything uses Circle Projector except for Directional Targeting
            if (ConeProjector != null) ConeProjector.gameObject.SetActive(false);

            if (CircleProjector != null) 
            {
                // Basic Cursor
                float displayRadius = e.Radius <= 0.1f ? 0.5f : e.Radius;

                CircleProjector.gameObject.SetActive(true);

                // Place at cursor position pointing straight down
                CircleProjector.transform.position = VectorAdapter.ToUnity(e.Position);
                CircleProjector.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

                // Diameter = Radius * 2
                float diameter = displayRadius * 2f;
                CircleProjector.size = new Vector3(diameter, diameter, DecalDepth);

                CircleProjector.material = e.IsValid ? ValidCircleMaterial : InvalidCircleMaterial;
            }
        }
    }

    // ==========================================
    // RETAINED: PATH LINE RENDERER LOGIC
    // ==========================================
    private void UpdatePathVisuals(List<SimVector3> path, Color color)
    {
        if (PathLineRenderer == null) return;

        if (path == null || path.Count == 0)
        {
            PathLineRenderer.positionCount = 0;
            return;
        }

        PathLineRenderer.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
        {
            // Raycast the path points to ground them properly
            Vector3 point = GetGroundPosition(VectorAdapter.ToUnity(path[i]), out _);
            point.y += 0.1f; // Raise slightly above ground to prevent clipping
            PathLineRenderer.SetPosition(i, point);
        }

        PathLineRenderer.startColor = color;
        PathLineRenderer.endColor = color;
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

    // ==========================================
    // DEPRECATED / DEBUG: LEGACY VISUALS
    // ==========================================
    private void UpdateLegacyVisuals(CursorMovedEvent e, Vector3 targetPos, Vector3 targetNormal, Color color)
    {
        // 1. Legacy Cylinder Cursor
        if (LegacyCursorVisuals != null)
        {
            // Only show basic cursor if NOT directional
            if (e.Mode == TargetingMode.Directional)
            {
                LegacyCursorVisuals.SetActive(false);
            }
            else
            {
                LegacyCursorVisuals.SetActive(true);
                LegacyCursorVisuals.transform.position = targetPos + (targetNormal * 0.02f);
                LegacyCursorVisuals.transform.rotation = Quaternion.FromToRotation(Vector3.up, targetNormal);

                if (LegacyCursorRenderer != null)
                {
                    LegacyCursorRenderer.material.color = color;
                }
            }
        }

        // 2. Legacy AoE Line Renderer
        if (LegacyAoELineRenderer != null)
        {
            if (e.Radius <= 0)
            {
                LegacyAoELineRenderer.positionCount = 0;
                return;
            }

            switch(e.Mode)
            {
                case TargetingMode.PointAoE:
                case TargetingMode.ActorAoE:
                case TargetingMode.HybridAoE:
                    DrawLegacyAoECircle(targetPos, e.Radius, color);
                    break;

                case TargetingMode.Directional:
                    Vector3 centerPos = e.StaticCenter.HasValue ? GetGroundPosition(VectorAdapter.ToUnity(e.StaticCenter.Value), out _) : targetPos;
                    DrawLegacyAoECone(targetPos, centerPos, e.Radius, e.Angle, color);
                    break;

                default:
                    LegacyAoELineRenderer.positionCount = 0;
                    break;
            }
        }
    }

    private void DrawLegacyAoECircle(Vector3 center, float radius, Color color)
    {
        int segments = 36;
        LegacyAoELineRenderer.positionCount = segments + 1;
        LegacyAoELineRenderer.startColor = color;
        LegacyAoELineRenderer.endColor = color;
        float angleStep = 360f / segments;

        for (int i = 0; i <= segments; i++)
        {
            float rad = Mathf.Deg2Rad * (i * angleStep);
            Vector3 offset = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * radius;
            Vector3 point = center + offset;
            point.y += 0.05f;
            LegacyAoELineRenderer.SetPosition(i, point);
        }
    }

    private void DrawLegacyAoECone(Vector3 cursorPos, Vector3 staticCenter, float radius, float angle, Color color)
    {
        Vector3 forwardDir = Vector3.forward;
        if (Vector3.Distance(staticCenter, cursorPos) > 0.01f)
        {
            forwardDir = cursorPos - staticCenter;
            forwardDir.y = 0;
            forwardDir.Normalize();
        }

        int segments = 20;
        LegacyAoELineRenderer.positionCount = segments + 2; 
        LegacyAoELineRenderer.startColor = color;
        LegacyAoELineRenderer.endColor = color;

        float startAngle = -angle / 2f;
        float angleStep = angle / segments;

        Vector3 raisedCenter = staticCenter + new Vector3(0, 0.05f, 0);
        LegacyAoELineRenderer.SetPosition(0, raisedCenter);

        for (int i = 0; i <= segments; i++)
        {
           float currentAngle = startAngle + (i * angleStep);
           Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
           Vector3 direction = rotation * forwardDir;

           Vector3 point = staticCenter + (direction * radius);
           point.y += 0.05f;
           LegacyAoELineRenderer.SetPosition(i + 1, point);
        }
        LegacyAoELineRenderer.loop = true; 
    }

    private void OnDestroy()
    {
        simulation.Events.Unsubscribe<CursorMovedEvent>(OnCursorMoved);
    }
}