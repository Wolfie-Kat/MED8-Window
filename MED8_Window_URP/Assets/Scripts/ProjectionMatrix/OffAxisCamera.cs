using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OffAxisCamera : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The projection plane representing your physical monitor")]
    public ProjectionPlane projectionPlane;

    [Tooltip("The transform representing the viewer's eye position")]
    public Transform eyeTransform;

    [Header("Clip Planes")]
    public float nearClip = 0.05f;
    public float farClip = 100f;

    [Header("Debug — Eye to Projection Plane")]
    [Tooltip("Perpendicular distance from the eye to the projection plane (meters)")]
    [SerializeField] private float eyeDistance;

    [Header("Debug — Frustum Extents")]
    [Tooltip("Left/Right/Bottom/Top extents of the projection plane as seen from the eye, scaled to the near clip plane")]
    [SerializeField] private float frustumLeft;
    [SerializeField] private float frustumRight;
    [SerializeField] private float frustumBottom;
    [SerializeField] private float frustumTop;

    private Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (projectionPlane == null || eyeTransform == null)
            return;

        // Step 1: perpendicular distance from eye to projection plane.
        // We pick any corner (bottom-left) and compute the vector from
        // the eye to that corner. Then we project that vector onto the
        // projection plane's normal to get the perpendicular distance.
        Vector3 eyeToCorner = projectionPlane.BottomLeft - eyeTransform.position;
        eyeDistance = -Vector3.Dot(eyeToCorner, projectionPlane.DirNormal);

        if (eyeDistance < 0.001f)
            return; // eye is on or behind the projection plane

        // Step 2: frustum extents.
        // For each edge we need, take the vector from eye to the relevant
        // corner and project it onto the projection plane's axes.
        //
        // Bottom-left corner gives us: left extent (DirRight) and bottom extent (DirUp)
        // Bottom-right corner gives us: right extent (DirRight)
        // Top-left corner gives us: top extent (DirUp)
        Vector3 eyePos = eyeTransform.position;
        Vector3 eyeToBL = projectionPlane.BottomLeft - eyePos;
        Vector3 eyeToBR = projectionPlane.BottomRight - eyePos;
        Vector3 eyeToTL = projectionPlane.TopLeft - eyePos;

        float nearOverDist = nearClip / eyeDistance;

        frustumLeft   = Vector3.Dot(eyeToBL, projectionPlane.DirRight) * nearOverDist;
        frustumRight  = Vector3.Dot(eyeToBR, projectionPlane.DirRight) * nearOverDist;
        frustumBottom = Vector3.Dot(eyeToBL, projectionPlane.DirUp)    * nearOverDist;
        frustumTop    = Vector3.Dot(eyeToTL, projectionPlane.DirUp)    * nearOverDist;

        // Step 3: build and apply the projection matrix.
        // This is an asymmetric frustum — the pyramid of vision is NOT
        // centered on the camera's forward axis. Instead it's skewed so
        // it passes exactly through the projection plane edges.
        _cam.projectionMatrix = Matrix4x4.Frustum(
            frustumLeft, frustumRight, frustumBottom, frustumTop,
            nearClip, farClip
        );

        // Step 4: build and apply the view matrix.
        // Normally Unity builds this from the camera's transform.rotation.
        // We override it with two parts:
        //
        // M — rotates world space so that:
        //     projection plane's right  axis → camera X
        //     projection plane's up     axis → camera Y
        //     projection plane's normal axis → camera Z
        //   This means the camera always "faces" perpendicular to the
        //   projection plane, regardless of the camera GameObject's rotation.
        //
        // T — translates by negative eye position (moves the world so the
        //     eye is at the origin).
        //
        // Combined: worldToCameraMatrix = M * T
        Matrix4x4 M = projectionPlane.M;
        Matrix4x4 T = Matrix4x4.Translate(-eyePos);
        _cam.worldToCameraMatrix = M * T;
    }
}
