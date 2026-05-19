using UnityEngine;

/// <summary>
/// Defines a screen rectangle in 3D space.
/// All sizes are in meters (1 Unity unit = 1 meter).
/// </summary>
[ExecuteInEditMode]
public class ProjectionPlane : MonoBehaviour
{
    [Header("Screen Size (meters)")]
    [Tooltip("Physical screen width and height in meters. E.g. 27\" monitor = (0.60, 0.34)")]
    public Vector2 SizeInMeters = new Vector2(0.60f, 0.34f);

    // Screen corners in world space
    public Vector3 BottomLeft { get; private set; }
    public Vector3 BottomRight { get; private set; }
    public Vector3 TopLeft { get; private set; }
    public Vector3 TopRight { get; private set; }

    // Screen axes
    public Vector3 DirRight { get; private set; }
    public Vector3 DirUp { get; private set; }
    public Vector3 DirNormal { get; private set; }

    // Rotation matrix: world -> screen-local space
    public Matrix4x4 M { get; private set; }

    [Header("Window Frame")]
    [Tooltip("Show a thick frame around the projection plane to sell the window illusion")]
    public bool ShowWindowFrame = false;
    [Tooltip("How thick the wall is (depth into the scene, meters)")]
    public float FrameDepth = 0.15f;
    [Tooltip("How wide the border extends beyond the screen edges (meters)")]
    public float FrameBorder = 0.3f;
    public Color FrameColor = new Color(0.35f, 0.33f, 0.30f);

    private GameObject _frameRoot;

    void Update()
    {
        SizeInMeters.x = Mathf.Max(0.01f, SizeInMeters.x);
        SizeInMeters.y = Mathf.Max(0.01f, SizeInMeters.y);
        Vector2 half = SizeInMeters * 0.5f;

        BottomLeft  = transform.TransformPoint(new Vector3(-half.x, -half.y, 0));
        BottomRight = transform.TransformPoint(new Vector3( half.x, -half.y, 0));
        TopLeft     = transform.TransformPoint(new Vector3(-half.x,  half.y, 0));
        TopRight    = transform.TransformPoint(new Vector3( half.x,  half.y, 0));

        DirRight  = (BottomRight - BottomLeft).normalized;
        DirUp     = (TopLeft - BottomLeft).normalized;
        DirNormal = -Vector3.Cross(DirRight, DirUp).normalized;

        var m = Matrix4x4.zero;
        m[0, 0] = DirRight.x;  m[0, 1] = DirRight.y;  m[0, 2] = DirRight.z;
        m[1, 0] = DirUp.x;     m[1, 1] = DirUp.y;     m[1, 2] = DirUp.z;
        m[2, 0] = DirNormal.x; m[2, 1] = DirNormal.y; m[2, 2] = DirNormal.z;
        m[3, 3] = 1.0f;
        M = m;

        if (Application.isPlaying)
            UpdateWindowFrame();
    }

    void UpdateWindowFrame()
    {
        if (ShowWindowFrame && _frameRoot == null)
            BuildFrame();
        if (!ShowWindowFrame && _frameRoot != null)
        {
            Destroy(_frameRoot);
            _frameRoot = null;
        }
        if (_frameRoot != null)
            PositionFrame();
    }

    void BuildFrame()
    {
        _frameRoot = new GameObject("WindowFrame");
        _frameRoot.transform.SetParent(transform, false);

        // 4 outer walls + 4 inner bevels = 8 cubes
        string[] names = { "Top", "Bottom", "Left", "Right",
                           "Bevel_Top", "Bevel_Bottom", "Bevel_Left", "Bevel_Right" };
        Color bevelColor = new Color(FrameColor.r + 0.08f, FrameColor.g + 0.06f, FrameColor.b + 0.05f);
        for (int i = 0; i < 8; i++)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Frame_" + names[i];
            wall.transform.SetParent(_frameRoot.transform, false);
            wall.GetComponent<Renderer>().material.color = i < 4 ? FrameColor : bevelColor;
        }
    }

    void PositionFrame()
    {
        if (_frameRoot.transform.childCount < 8) return;

        float hw = SizeInMeters.x * 0.5f;
        float hh = SizeInMeters.y * 0.5f;
        float d = FrameDepth;
        float b = FrameBorder;
        float bevelThick = 0.005f; // thin slab for the inner face

        // --- Outer walls (behind the screen, surrounding the opening) ---

        var top = _frameRoot.transform.GetChild(0);
        top.localScale = new Vector3(SizeInMeters.x + b * 2f, b, d);
        top.localPosition = new Vector3(0, hh + b * 0.5f, d * 0.5f);

        var bot = _frameRoot.transform.GetChild(1);
        bot.localScale = new Vector3(SizeInMeters.x + b * 2f, b, d);
        bot.localPosition = new Vector3(0, -hh - b * 0.5f, d * 0.5f);

        var left = _frameRoot.transform.GetChild(2);
        left.localScale = new Vector3(b, SizeInMeters.y + b * 2f, d);
        left.localPosition = new Vector3(-hw - b * 0.5f, 0, d * 0.5f);

        var right = _frameRoot.transform.GetChild(3);
        right.localScale = new Vector3(b, SizeInMeters.y + b * 2f, d);
        right.localPosition = new Vector3(hw + b * 0.5f, 0, d * 0.5f);

        // --- Inner bevels (the visible inner edges of the wall opening) ---
        // These face inward and run along the depth of the wall.
        // They're what you see when you look at the "cut" edge of the wall.

        // Top bevel: horizontal strip along the top edge of the opening
        var bTop = _frameRoot.transform.GetChild(4);
        bTop.localScale = new Vector3(SizeInMeters.x, bevelThick, d);
        bTop.localPosition = new Vector3(0, hh, d * 0.5f);

        // Bottom bevel
        var bBot = _frameRoot.transform.GetChild(5);
        bBot.localScale = new Vector3(SizeInMeters.x, bevelThick, d);
        bBot.localPosition = new Vector3(0, -hh, d * 0.5f);

        // Left bevel: vertical strip along the left edge of the opening
        var bLeft = _frameRoot.transform.GetChild(6);
        bLeft.localScale = new Vector3(bevelThick, SizeInMeters.y, d);
        bLeft.localPosition = new Vector3(-hw, 0, d * 0.5f);

        // Right bevel
        var bRight = _frameRoot.transform.GetChild(7);
        bRight.localScale = new Vector3(bevelThick, SizeInMeters.y, d);
        bRight.localPosition = new Vector3(hw, 0, d * 0.5f);
    }

    void OnDestroy()
    {
        if (_frameRoot != null)
            DestroyImmediate(_frameRoot);
    }

    private void OnDrawGizmos()
    {
        Vector3 center = (BottomLeft + TopRight) * 0.5f;
        float hw = SizeInMeters.x * 0.5f;
        float hh = SizeInMeters.y * 0.5f;

        // Screen rectangle
        Gizmos.color = Color.red;
        Gizmos.DrawLine(BottomLeft, BottomRight);
        Gizmos.DrawLine(BottomRight, TopRight);
        Gizmos.DrawLine(TopRight, TopLeft);
        Gizmos.DrawLine(TopLeft, BottomLeft);

        // Diagonal cross so it's visible from far away
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawLine(BottomLeft, TopRight);
        Gizmos.DrawLine(BottomRight, TopLeft);

        // Normal arrow (shows which side the eye should be on)
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(center, center + DirNormal * 0.3f);
        // Arrowhead
        Vector3 arrowTip = center + DirNormal * 0.3f;
        Gizmos.DrawLine(arrowTip, arrowTip - DirNormal * 0.05f + DirRight * 0.03f);
        Gizmos.DrawLine(arrowTip, arrowTip - DirNormal * 0.05f - DirRight * 0.03f);

        // Axes labels: R=right, U=up
        Gizmos.color = Color.green;
        Gizmos.DrawLine(center, center + DirRight * hw);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(center, center + DirUp * hh);

        // "Eye side" label — small sphere on the normal side
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center + DirNormal * 0.15f, 0.02f);
    }
}
