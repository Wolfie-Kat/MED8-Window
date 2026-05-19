using UnityEngine;

/// <summary>
/// Converts head tracking data from the Python UDP connector
/// into a world-space eye position for the off-axis projection.
///
/// All internal math is in meters. The only cm value is connector.distance
/// which is converted to meters immediately on read.
/// </summary>
public class HeadTracker : MonoBehaviour
{
    [Header("References")]
    public UnityPythonConnector connector;
    public ProjectionPlane projectionPlane;

    [Header("Physical Setup")]
    [Tooltip("Fallback sitting distance in meters (used when tracker reports 0)")]
    public float FallbackDistance = 0.6f;
    [Tooltip("Webcam horizontal FOV in degrees")]
    public float WebcamFOV = 65f;

    [Header("Tuning")]
    [Tooltip("Scales the head movement. 1.0 = physically accurate")]
    [Range(0.1f, 3f)]
    public float Sensitivity = 1.0f;
    [Tooltip("Smoothing. Higher = smoother but laggier")]
    [Range(1f, 30f)]
    public float Smoothing = 10f;

    [Header("Debug — all values in meters")]
    [SerializeField] private float eyeDistanceFromPlane;
    [SerializeField] private Vector3 headOffsetMeters;
    [SerializeField] private Vector3 eyeWorldPosition;
    [SerializeField] private bool trackingActive;

    private Vector3 _smoothedOffset;
    private bool _initialized;

    void Start()
    {
        if (projectionPlane != null)
        {
            Vector3 center = projectionPlane.transform.position;
            transform.position = center + projectionPlane.DirNormal * FallbackDistance;
        }
    }

    void Update()
    {
        if (connector == null || projectionPlane == null)
            return;

        trackingActive = connector.receivedValue != Vector2.zero || connector.distance > 0f;

        // connector.distance is in cm — convert to meters immediately
        float dist = connector.distance > 0f
            ? connector.distance * 0.01f
            : FallbackDistance;

        // Convert normalized webcam coords (0-1) to meters
        float halfTanH = Mathf.Tan(WebcamFOV * 0.5f * Mathf.Deg2Rad);
        float aspectRatio = projectionPlane.SizeInMeters.y / projectionPlane.SizeInMeters.x;
        float halfTanV = halfTanH * aspectRatio;

        float headX = (0.5f - connector.receivedValue.x) * 2f * halfTanH * dist * Sensitivity;
        float headY = (0.5f - connector.receivedValue.y) * 2f * halfTanV * dist * Sensitivity;

        Vector3 targetOffset = new Vector3(headX, headY, dist);

        if (!_initialized)
        {
            _smoothedOffset = targetOffset;
            _initialized = true;
        }
        _smoothedOffset = Vector3.Lerp(_smoothedOffset, targetOffset, Smoothing * Time.deltaTime);

        headOffsetMeters = _smoothedOffset;
        eyeDistanceFromPlane = _smoothedOffset.z;

        // Position eye relative to projection plane center
        Vector3 center = projectionPlane.transform.position;
        Vector3 newPos = center
            + projectionPlane.DirRight  * _smoothedOffset.x
            + projectionPlane.DirUp     * _smoothedOffset.y
            + projectionPlane.DirNormal * _smoothedOffset.z;
        transform.position = newPos;
        eyeWorldPosition = newPos;
    }
}
