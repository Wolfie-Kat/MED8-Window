using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] private UnityPythonConnector gameManager;
    [SerializeField] private ProjectionPlaneCamera projectionCamera;
    [SerializeField] private GameObject projectionPlane;

    [Header("1. Your Physical Setup (measure these)")]
    [Tooltip("Width of your physical monitor in centimeters. Measure it with a ruler.")]
    public float ScreenWidthCm = 60f;
    [Tooltip("Height of your physical monitor in centimeters. Measure it with a ruler.")]
    public float ScreenHeightCm = 34f;
    [Tooltip("How far you sit from your monitor in centimeters.")]
    public float SittingDistanceCm = 60f;
    [Tooltip("Horizontal FOV of your webcam in degrees. Read from the Python console on startup.")]
    public float WebcamFOV = 65f;

    [Header("2. Tuning")]
    [Tooltip("How much head movement affects the view. Start at 1.0, lower if it feels too strong.")]
    [Range(0.1f, 2f)]
    public float Sensitivity = 1.0f;
    [Tooltip("Smoothing amount. Higher = smoother but laggier.")]
    [Range(1f, 30f)]
    public float Smoothing = 10f;

    [Header("3. Debug (read-only at runtime)")]
    public Vector3 HeadOffset;

    private Transform _headTransform;
    private Vector3 _smoothedOffset;
    private bool _initialized;

    void Start()
    {
        _headTransform = transform;
        if (projectionCamera != null)
            projectionCamera.HeadPosition = _headTransform;
    }

    void Update()
    {
        if (projectionPlane == null || projectionCamera == null) return;
        var screen = projectionCamera.ProjectionScreen;
        if (screen == null) return;

        // World scale: how many scene units = 1 cm of real-world movement.
        // Separate horizontal and vertical scales so any screen shape maps correctly.
        float worldScaleH = screen.Size.x / ScreenWidthCm;
        float worldScaleV = screen.Size.y / ScreenHeightCm;

        // Convert webcam normalized coordinates (0..1) to head offset in cm.
        // Perspective is linear in tan, not angle: offset = (0.5 - norm) * 2 * tan(fov/2) * distance
        float halfTanH = Mathf.Tan(WebcamFOV * 0.5f * Mathf.Deg2Rad);
        float halfTanV = halfTanH * ScreenHeightCm / ScreenWidthCm;
        float headXcm = (0.5f - gameManager.receivedValue.x) * 2f * halfTanH * SittingDistanceCm;
        float headYcm = (0.5f - gameManager.receivedValue.y) * 2f * halfTanV * SittingDistanceCm;

        // Target offset in scene units
        Vector3 target = new Vector3(
            headXcm * worldScaleH * Sensitivity,
            headYcm * worldScaleV * Sensitivity,
            SittingDistanceCm * worldScaleH
        );

        // Smooth
        if (!_initialized) { _smoothedOffset = target; _initialized = true; }
        _smoothedOffset = Vector3.Lerp(_smoothedOffset, target, Smoothing * Time.deltaTime);
        HeadOffset = _smoothedOffset;

        // Place head relative to screen center along screen axes
        Vector3 center = projectionPlane.transform.position;
        _headTransform.position = center
            + screen.DirRight * _smoothedOffset.x
            + screen.DirUp * _smoothedOffset.y
            + screen.DirNormal * _smoothedOffset.z;
    }
}
