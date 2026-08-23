using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Rig Settings")]
    [Tooltip("Drag the child Camera object here")]
    public Transform CameraChild; 
    public float HeightPivotOffset = 1.5f; // Looks at the actors' chests, not their feet

    [Header("Zoom Settings")]
    public float ZoomDistance = 12f;
    public float ZoomSpeed = 30f;
    public float MinZoom = 12f;
    public float MaxZoom = 20f;

    [Header("Orbit Settings")]
    public float OrbitSpeed = 120f;
    public float PitchSpeed = 60f;
    public float MinPitch = 15f; // Low to the ground, looking forward
    public float MaxPitch = 85f; // High in the air, looking straight down

    [Header("Edge Pan Settings")]
    public float PanSpeed = 15f;
    public float EdgeThreshold = 0.05f; // 5% of screen edge

    private float currentYaw;
    private float currentPitch = 45f; // Start at a nice isometric angle
    private Camera cam;

    // TODO: Add functions/coroutines to smoothly change position
    // e.g. smooth pan, smoothly rotate and lookat, smooth move to position, follow, etc.

    // Explicit Initialization called by the Bootstrapper
    public void Initialize()
    {
        cam = GetComponentInChildren<Camera>();
        
        // Auto-assign the child if you forgot to drag it in
        if (CameraChild == null && cam != null)
        {
            CameraChild = cam.transform;
        }

        currentYaw = transform.eulerAngles.y;
        UpdateCameraTransform();
    }

    // Called by the Input Manager via left stick
    public void RotateCamera(float x, float y, float deltaTime)
    {
        // 1. Calculate new angles
        currentYaw += x * OrbitSpeed * deltaTime;
        
        currentPitch -= y * PitchSpeed * deltaTime;
        currentPitch = Mathf.Clamp(currentPitch, MinPitch, MaxPitch);

        // 2. Apply transformations
        UpdateCameraTransform();
    }

    // Called by the Input Manager via Triggers, Bumpers, or Scroll Wheel
    public void ZoomCamera(float zoomInput, float deltaTime)
    {
        if (Mathf.Abs(zoomInput) < 0.01f) return;

        // If zoomInput is positive, we zoom IN (decrease distance)
        // If zoomInput is negative, we zoom OUT (increase distance)
        ZoomDistance -= zoomInput * ZoomSpeed * deltaTime;
        ZoomDistance = Mathf.Clamp(ZoomDistance, MinZoom, MaxZoom);

        UpdateCameraTransform();
    }

    private void UpdateCameraTransform()
    {
        // The Root Pivot handles Yaw (Left/Right rotation)
        transform.rotation = Quaternion.Euler(0, currentYaw, 0);

        if (CameraChild != null)
        {
            // The Child Camera handles Pitch locally
            CameraChild.localRotation = Quaternion.Euler(currentPitch, 0, 0);
            
            // Offset the camera backward along its local Z axis to create the Spherical Orbit
            CameraChild.localPosition = new Vector3(0, HeightPivotOffset, 0) + (CameraChild.localRotation * new Vector3(0, 0, -ZoomDistance));
        }
    }

    // Called by the State Machine via Left Stick / Cursor position
    public void HandleEdgePan(Vector3 cursorWorldPosition)
    {
        if (cam == null) return;

        Vector3 viewportPos = cam.WorldToViewportPoint(cursorWorldPosition);
        Vector3 panDirection = Vector3.zero;

        // Flatten the camera's forward vector so we pan parallel to the ground
        Vector3 flatForward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        Vector3 flatRight = transform.right;

        // Check if cursor is pushing the edges
        if (viewportPos.x < EdgeThreshold) panDirection -= flatRight;
        if (viewportPos.x > 1f - EdgeThreshold) panDirection += flatRight;
        if (viewportPos.y < EdgeThreshold) panDirection -= flatForward;
        if (viewportPos.y > 1f - EdgeThreshold) panDirection += flatForward;

        if (panDirection.sqrMagnitude > 0.01f)
        {
            transform.position += panDirection.normalized * PanSpeed * Time.deltaTime;
        }
    }
}