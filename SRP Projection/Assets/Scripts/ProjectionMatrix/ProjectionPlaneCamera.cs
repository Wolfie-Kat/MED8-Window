using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ProjectionPlaneCamera : MonoBehaviour
{
    [Header("Projection plane")]
    public ProjectionPlane ProjectionScreen;
    public bool ClampNearPlane = true;

    [Header("Tracking")]
    public Transform HeadPosition;

    [Header("Helpers")]
    public bool DrawGizmos = true;

    private Vector3 eyePos;
    private float _n, _f;
    private Vector3 va, vb, vc, vd;
    private float _l, _r, _b, _t;
    private Vector3 _screenCenter;
    private Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    private void OnDrawGizmos()
    {
        if (ProjectionScreen == null || !DrawGizmos)
            return;

        var pos = eyePos;

        // Lines from eye to screen corners
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, pos + va);
        Gizmos.DrawLine(pos, pos + vb);
        Gizmos.DrawLine(pos, pos + vc);
        Gizmos.DrawLine(pos, pos + vd);

        // Line from eye to screen center
        Gizmos.color = Color.white;
        Gizmos.DrawLine(pos, _screenCenter);
    }

    void LateUpdate()
    {
        if (ProjectionScreen == null)
            return;

        // Screen corners in world space
        Vector3 pa = ProjectionScreen.BottomLeft;
        Vector3 pb = ProjectionScreen.BottomRight;
        Vector3 pc = ProjectionScreen.TopLeft;
        Vector3 pd = ProjectionScreen.TopRight;

        // Screen orthonormal basis
        Vector3 vr = ProjectionScreen.DirRight;
        Vector3 vu = ProjectionScreen.DirUp;
        Vector3 vn = ProjectionScreen.DirNormal;

        // Eye position comes from head tracker, fallback to this transform
        eyePos = HeadPosition != null ? HeadPosition.position : transform.position;

        // Vectors from eye to each screen corner
        va = pa - eyePos;
        vb = pb - eyePos;
        vc = pc - eyePos;
        vd = pd - eyePos;

        _screenCenter = (pa + pb + pc + pd) * 0.25f;

        // Perpendicular distance from eye to the screen plane
        float d = -Vector3.Dot(va, vn);

        // Prevent degenerate frustum if eye is on or behind the screen
        if (d < 0.01f)
            return;

        if (ClampNearPlane)
        {
            _cam.nearClipPlane = d;
        }
        _n = _cam.nearClipPlane;
        _f = _cam.farClipPlane;

        // Scale factor to project screen extents onto the near plane
        float nearOverDist = _n / d;

        // Project corner vectors onto screen axes, scaled to near plane
        _l = Vector3.Dot(vr, va) * nearOverDist;
        _r = Vector3.Dot(vr, vb) * nearOverDist;
        _b = Vector3.Dot(vu, va) * nearOverDist;
        _t = Vector3.Dot(vu, vc) * nearOverDist;

        // Off-axis projection matrix
        _cam.projectionMatrix = Matrix4x4.Frustum(_l, _r, _b, _t, _n, _f);

        // View matrix: rotate world into screen-aligned space, then translate
        // This is the Kooima generalized perspective projection view matrix.
        // Row 0 = screen right, Row 1 = screen up, Row 2 = screen normal
        // The screen axes define the camera orientation — the camera transform's
        // own rotation is irrelevant and must NOT be mixed in.
        Matrix4x4 M = ProjectionScreen.M;
        Matrix4x4 T = Matrix4x4.Translate(-eyePos);
        _cam.worldToCameraMatrix = M * T;
    }
}
