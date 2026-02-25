using System;
using UnityEngine;

[ExecuteInEditMode]
public class ProjectionPlane : MonoBehaviour
{
    [Header("Size")] 
    public Vector2 Size = new Vector2(8f, 4.5f);
    public Vector2 AspectRatio = new Vector2(16, 9);
    public bool LockAspectRatio = true;
    [Header("Vizualization")]
    public bool DrawGizmos = true;
    [Header("Alignment")] 
    public bool ShowAlignmentCube = false;
    public float AlignmentDepth = 5;
    public Material AlignmentMaterial;
    
    public Vector3 BottomLeft { get; private set; }
    public Vector3 BottomRight { get; private set; }
    public Vector3 TopLeft { get; private set; }
    public Vector3 TopRight { get; private set; }
    
    public Vector3 DirRight { get; private set; }
    public Vector3 DirUp { get; private set; }
    public Vector3 DirNormal { get; private set; }

    private Vector2 _previousSize = new Vector2(8, 4.5f);
    private Vector2 _previousAspectRatio = new Vector2(16, 9);

    private GameObject _alignmentCube;
    private Transform _backTransform;
    private Transform _leftTransform;
    private Transform _rightTransform;
    private Transform _topTransform;
    private Transform _bottomTransform;

    private Matrix4x4 m;
    public Matrix4x4 M { get => m; }

    private void OnDrawGizmos()
    {
        if (DrawGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(BottomLeft, BottomRight);
            Gizmos.DrawLine(BottomLeft, TopLeft);
            Gizmos.DrawLine(TopRight, BottomRight);
            Gizmos.DrawLine(TopLeft, TopRight);
            
            //Draw direction towards eye
            Gizmos.color = Color.cyan;
            var planeCenter = BottomLeft + (TopRight - BottomLeft) * 0.5f;
            Gizmos.DrawLine(planeCenter, planeCenter + DirNormal);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Application.isPlaying)
        {
            _alignmentCube = new GameObject("AlignmentCube");
            _alignmentCube.transform.SetParent(transform, false);

            _alignmentCube.transform.localPosition = Vector3.zero;
            _alignmentCube.transform.rotation = transform.rotation;

            GameObject back = CreateAlignmentQuad();
            _backTransform = back.transform;
            GameObject left = CreateAlignmentQuad();
            _leftTransform = left.transform;
            GameObject right = CreateAlignmentQuad();
            _rightTransform = right.transform;
            GameObject top = CreateAlignmentQuad();
            _topTransform = top.transform;
            GameObject bottom = CreateAlignmentQuad();
            _bottomTransform = bottom.transform;
        }
    }

    private GameObject CreateAlignmentQuad()
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.parent = _alignmentCube.transform;
        quad.GetComponent<Renderer>().material = AlignmentMaterial;
        return quad;
    }

    public void UpdateAlignmentCube()
    {
        Vector2 halfSize = Size * 0.5f;
        UpdateAlignmentQuad(_backTransform, new Vector3(0, 0, AlignmentDepth), new Vector3(Size.x, Size.y), 
            Quaternion.identity);
        
        UpdateAlignmentQuad(_leftTransform, new Vector3(-halfSize.x, 0, AlignmentDepth * 0.5f),
            new Vector3(AlignmentDepth, Size.y, 0),
            Quaternion.Euler(0, -90, 0));
        
        UpdateAlignmentQuad(_rightTransform,
            new Vector3(halfSize.x, 0, AlignmentDepth * 0.5f), new Vector3(AlignmentDepth, Size.y, 0),
            Quaternion.Euler(0, 90, 0));
        
        UpdateAlignmentQuad(_topTransform,
            new Vector3(0, halfSize.y, AlignmentDepth * 0.5f), new Vector3(Size.x, AlignmentDepth, 0),
            Quaternion.Euler(-90, 0, 0));
        
        UpdateAlignmentQuad(_bottomTransform,
            new Vector3(0, -halfSize.y, AlignmentDepth * 0.5f), new Vector3(Size.x, AlignmentDepth, 0),
            Quaternion.Euler(90, 0, 0));
    }

    private void UpdateAlignmentQuad(Transform t, Vector3 pos, Vector3 scale, Quaternion rotation)
    {
        t.localPosition = pos;
        t.localScale = scale;
        t.localRotation = rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isPlaying)
        {
            _alignmentCube.SetActive(ShowAlignmentCube);
            if (_alignmentCube.activeInHierarchy)
            {
                UpdateAlignmentCube();
            }
        }
        
        //Do aspect ratio contraints
        if (LockAspectRatio)
        {
            if (AspectRatio.x != _previousAspectRatio.x)
            {
                Size.y = Size.x / AspectRatio.x * AspectRatio.y;
                //make X dominant axis - i.e. if both change, X takes precedence
                _previousAspectRatio.y = AspectRatio.y;
            }

            if (AspectRatio.y != _previousAspectRatio.y)
            {
                Size.x = Size.y / AspectRatio.y * AspectRatio.x;
            }

            if (Size.x != _previousSize.x)
            {
                Size.y = Size.x / AspectRatio.x * AspectRatio.y;
                //make X dominant axis - i.e. if both change, X takes precedence
                _previousSize.y = Size.y;
            }

            if (Size.y != _previousSize.y)
            {
                Size.x = Size.y / AspectRatio.y * AspectRatio.x;
            }
        }
        
        // Don't crash Unity
        Size.x = Mathf.Max(1, Size.x);
        Size.y = Mathf.Max(1, Size.y);
        AspectRatio.x = Mathf.Max(1, AspectRatio.x);
        AspectRatio.y = Mathf.Max(1, AspectRatio.y);

        _previousSize = Size;
        _previousAspectRatio = AspectRatio;

        BottomLeft = transform.TransformPoint(new Vector3(-Size.x, -Size.y) * 0.5f);
        BottomRight = transform.TransformPoint(new Vector3(Size.x, -Size.y) * 0.5f);
        TopLeft = transform.TransformPoint(new Vector3(-Size.x, Size.y) * 0.5f);
        TopRight = transform.TransformPoint(new Vector3(Size.x, Size.y) * 0.5f);

        DirRight = (BottomRight - BottomLeft).normalized;
        DirUp = (TopLeft - BottomLeft).normalized;
        DirNormal = -Vector3.Cross(DirRight, DirUp).normalized;
        
        m = Matrix4x4.zero;
        m[0, 0] = DirRight.x;
        m[0, 1] = DirRight.y;
        m[0, 2] = DirRight.z;

        m[1, 0] = DirUp.x;
        m[1, 1] = DirUp.y;
        m[1, 2] = DirUp.z;

        m[2, 0] = DirNormal.x;
        m[2, 1] = DirNormal.y;
        m[2, 2] = DirNormal.z;

        m[3, 3] = 1.0f;
    }

    private void OnApplicationQuit()
    {
        if (Application.isPlaying && _alignmentCube != null)
        {
            DestroyImmediate(_alignmentCube);
        }
    }
}
