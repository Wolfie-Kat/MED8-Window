using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ProjectionPlaneCamera : MonoBehaviour
{
    [Header("Projection plane")] 
    public ProjectionPlane ProjectionScreen;
    public bool ClampNearPlane = true;
    [Header("Helpers")] 
    public bool DrawGizmos = true;

    private Vector3 eyePos;
    //From eye to projection screen corners
    private float _n, _f;

    private Vector3 va, vb, vc, vd;
    
    //Extents of perpendicular projection
    private float _l, _r, _b, _t;

    private Vector3 _viewDir;

    private Camera _cam;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    
    private void OnDrawGizmos()
    {
        if (ProjectionScreen == null)
            return;

        if (DrawGizmos)
        {
            var pos = transform.position;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pos, pos + va);
            Gizmos.DrawLine(pos, pos + vb);
            Gizmos.DrawLine(pos, pos + vc);
            Gizmos.DrawLine(pos, pos + vd);

            Vector3 pa = ProjectionScreen.BottomLeft;
            Vector3 vr = ProjectionScreen.DirRight;
            Vector3 vu = ProjectionScreen.DirUp;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(pos, _viewDir);
        }
    }
    
    
    
    // Update is called once per frame
    void LateUpdate()
    {
        if (ProjectionScreen != null)
        {
            Vector3 pa = ProjectionScreen.BottomLeft;
            Vector3 pb = ProjectionScreen.BottomRight;
            Vector3 pc = ProjectionScreen.TopLeft;
            Vector3 pd = ProjectionScreen.TopRight;

            Vector3 vr = ProjectionScreen.DirRight;
            Vector3 vu = ProjectionScreen.DirUp;
            Vector3 vn = ProjectionScreen.DirNormal;

            Matrix4x4 M = ProjectionScreen.M;

            eyePos = transform.position;
            
            //From eye to projection screen corners
            va = pa - eyePos;
            vb = pb - eyePos;
            vc = pc - eyePos;
            vd = pd - eyePos;

            _viewDir = eyePos + va + vb + vc + vd;
            
            //distance from eye to projection screen plane
            float d = -Vector3.Dot(va, vn);
            if (ClampNearPlane)
            {
                _cam.nearClipPlane = d;
            }
            _n = _cam.nearClipPlane;
            _f = _cam.farClipPlane;

            float nearOverDist = _n / d;

            _l = Vector3.Dot(vr, va) * nearOverDist;
            _r = Vector3.Dot(vr, vb) * nearOverDist;
            _b = Vector3.Dot(vu, va) * nearOverDist;
            _t = Vector3.Dot(vu, vc) * nearOverDist;
            Matrix4x4 P = Matrix4x4.Frustum(_l, _r, _b, _t, _n, _f);
            
            //Translation to eye position
            Matrix4x4 T = Matrix4x4.Translate(-eyePos);

            Matrix4x4 R = Matrix4x4.Rotate(Quaternion.Inverse(transform.rotation) * ProjectionScreen.transform.rotation);

            _cam.worldToCameraMatrix = M * R * T;

            _cam.projectionMatrix = P;
        }
    }
}
