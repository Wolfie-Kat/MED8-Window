using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ProjectionPlaneCamera : MonoBehaviour
{
    public enum ProjectionMode
    {
        FullMatrixMode,  // Your original implementation
        SimpleMode       // HTML/Three.js style implementation
    }
    
    [Header("Mode Selection")]
    public ProjectionMode CurrentMode = ProjectionMode.FullMatrixMode;
    
    [Header("Projection plane")] 
    public ProjectionPlane ProjectionScreen;
    public bool ClampNearPlane = true;
    
    [Header("Simple Mode Settings (HTML Style)")]
    public Vector2 ScreenSize = new Vector2(30f, 20f);
    public float NearClip = 0.5f;
    public float FarClip = 1000f;
    
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
    
    [Header("Tracking")]
    public Transform HeadPosition;
    
    private ProjectionMode _lastMode;
    
    // Store original transform for mode switching
    private Vector3 _storedPosition;
    private Quaternion _storedRotation;
    
    
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
        else
        {
            // Simple mode gizmos - draw screen bounds
            if (HeadPosition != null)
            {
                Gizmos.color = Color.cyan;
                Vector3 screenCenter = ProjectionScreen ? ProjectionScreen.transform.position : Vector3.zero;
                Vector3 right = ProjectionScreen ? ProjectionScreen.transform.right : Vector3.right;
                Vector3 up = ProjectionScreen ? ProjectionScreen.transform.up : Vector3.up;
                
                Vector3 halfRight = right * (ScreenSize.x * 0.5f);
                Vector3 halfUp = up * (ScreenSize.y * 0.5f);
                
                Vector3 bl = screenCenter - halfRight - halfUp;
                Vector3 br = screenCenter + halfRight - halfUp;
                Vector3 tl = screenCenter - halfRight + halfUp;
                Vector3 tr = screenCenter + halfRight + halfUp;
                
                Gizmos.DrawLine(bl, br);
                Gizmos.DrawLine(br, tr);
                Gizmos.DrawLine(tr, tl);
                Gizmos.DrawLine(tl, bl);
                
                // Draw line from head to screen
                Gizmos.color = Color.green;
                Gizmos.DrawLine(HeadPosition.position, screenCenter);
            }
        }
    }
    
    void LateUpdate()
    {
        if (ProjectionScreen == null)
            return;
        
        if (_lastMode != CurrentMode)
        {
            OnModeChanged();
            _lastMode = CurrentMode;
        }

        switch (CurrentMode)
        {
            case ProjectionMode.FullMatrixMode:
                UpdateFullMatrixMode();
                break;
            case ProjectionMode.SimpleMode:
                UpdateSimpleMode();
                break;
        }
    }
    
    // Update is called once per frame
    void UpdateFullMatrixMode()
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
    
    void UpdateSimpleMode()
    {
        // HTML/Three.js style implementation
        if (HeadPosition == null)
        {
            Debug.LogWarning("Simple Mode: HeadPosition not assigned!");
            return;
        }
        
        // Get head position in screen's local space
        Vector3 headInScreenSpace = HeadPosition.position;
        if (ProjectionScreen != null)
        {
            headInScreenSpace = ProjectionScreen.transform.InverseTransformPoint(HeadPosition.position);
            // In screen space, screen is at origin, facing forward
        }
        
        float halfW = ScreenSize.x / 2f;
        float halfH = ScreenSize.y / 2f;
        
        // Head position relative to screen center (like HTML's head.x, head.y, head.z)
        float headX = headInScreenSpace.x;
        float headY = headInScreenSpace.y;
        float headZ = Mathf.Max(0.1f, headInScreenSpace.z); // Distance in front of screen
        
        // HTML-style calculation
        float scale = NearClip / headZ;
        
        float left = (-halfW - headX) * scale;
        float right = (halfW - headX) * scale;
        float bottom = (-halfH - headY) * scale;
        float top = (halfH - headY) * scale;
        
        // Set projection matrix (HTML style)
        _cam.projectionMatrix = PerspectiveOffCenter(left, right, bottom, top, NearClip, FarClip);
        
        // Set view matrix: camera at head position, looking at screen center
        if (ProjectionScreen != null)
        {
            _cam.transform.position = HeadPosition.position;
            _cam.transform.LookAt(ProjectionScreen.transform.position);
            _cam.worldToCameraMatrix = _cam.worldToCameraMatrix; // Unity handles this automatically
        }
    }
    
    void OnModeChanged()
    {
        if (CurrentMode == ProjectionMode.FullMatrixMode)
        {
            // Switching TO Full Mode - restore stored transform
            if (_storedPosition != Vector3.zero)
            {
                transform.position = _storedPosition;
                transform.rotation = _storedRotation;
            }
            // Reset camera matrices so Full Mode can recalculate
            _cam.ResetWorldToCameraMatrix();
            _cam.ResetProjectionMatrix();
        }
        else
        {
            // Switching TO Simple Mode - store current transform
            _storedPosition = transform.position;
            _storedRotation = transform.rotation;
        }
    }
    
    Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
    {
        float x = 2.0f * near / (right - left);
        float y = 2.0f * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2.0f * far * near) / (far - near);
        float e = -1.0f;
        
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x;   m[0, 1] = 0f; m[0, 2] = a;   m[0, 3] = 0f;
        m[1, 0] = 0f;  m[1, 1] = y;  m[1, 2] = b;   m[1, 3] = 0f;
        m[2, 0] = 0f;  m[2, 1] = 0f; m[2, 2] = c;   m[2, 3] = d;
        m[3, 0] = 0f;  m[3, 1] = 0f; m[3, 2] = e;   m[3, 3] = 0f;
        
        return m;
    }
}
