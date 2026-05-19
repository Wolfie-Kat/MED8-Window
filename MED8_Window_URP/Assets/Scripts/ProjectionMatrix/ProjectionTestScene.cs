using UnityEngine;

/// <summary>
/// Generates diagnostic geometry for testing off-axis projection correctness.
///
/// Key tests:
/// - Screen-plane markers (zero parallax): should stay fixed when eye moves
/// - Behind-screen objects (positive parallax): should shift opposite to eye
/// - In-front-of-screen objects (negative parallax): should shift with eye
/// - Grid lines: should remain straight (no warping)
/// - Corner markers: should align with viewport edges
/// </summary>
public class ProjectionTestScene : MonoBehaviour
{
    [Header("Screen Definition")]
    public Vector2 ScreenSize = new Vector2(4f, 2.25f);

    [Header("Settings")]
    public bool GenerateOnStart = true;

    private GameObject _root;

    void Start()
    {
        if (GenerateOnStart)
            Generate();
    }

    [ContextMenu("Generate Test Scene")]
    public void Generate()
    {
        if (_root != null) DestroyImmediate(_root);
        _root = new GameObject("--- PROJECTION TEST SCENE ---");

        float w = ScreenSize.x;
        float h = ScreenSize.y;
        Vector3 o = transform.position;
        Quaternion r = transform.rotation;

        // Colors
        Color red     = new Color(0.9f, 0.15f, 0.15f);
        Color green   = new Color(0.15f, 0.8f, 0.15f);
        Color blue    = new Color(0.2f, 0.3f, 0.9f);
        Color yellow  = new Color(0.95f, 0.9f, 0.1f);
        Color cyan    = new Color(0.1f, 0.85f, 0.85f);
        Color magenta = new Color(0.9f, 0.15f, 0.85f);
        Color white   = Color.white;
        Color gray    = new Color(0.4f, 0.4f, 0.4f);
        Color darkGray = new Color(0.2f, 0.2f, 0.2f);
        Color orange  = new Color(1f, 0.5f, 0.1f);

        float markerSize = Mathf.Min(w, h) * 0.02f;

        // =============================================================
        // 1) SCREEN PLANE MARKERS (z = 0) — ZERO PARALLAX
        //    These must NOT move relative to the viewport when the eye
        //    moves. If they shift, the projection is broken.
        // =============================================================

        // Corner markers — should align exactly with screen edges
        Box("ScreenCorner_BL", o, r, Vector3.one * markerSize,
            new Vector3(-w * 0.5f, -h * 0.5f, 0), red);
        Box("ScreenCorner_BR", o, r, Vector3.one * markerSize,
            new Vector3( w * 0.5f, -h * 0.5f, 0), green);
        Box("ScreenCorner_TL", o, r, Vector3.one * markerSize,
            new Vector3(-w * 0.5f,  h * 0.5f, 0), blue);
        Box("ScreenCorner_TR", o, r, Vector3.one * markerSize,
            new Vector3( w * 0.5f,  h * 0.5f, 0), yellow);

        // Center cross on screen plane
        Box("ScreenCenter", o, r, Vector3.one * markerSize * 1.5f,
            new Vector3(0, 0, 0), white);

        // Edge midpoint markers
        Box("ScreenMid_Top", o, r, Vector3.one * markerSize,
            new Vector3(0, h * 0.5f, 0), cyan);
        Box("ScreenMid_Bottom", o, r, Vector3.one * markerSize,
            new Vector3(0, -h * 0.5f, 0), cyan);
        Box("ScreenMid_Left", o, r, Vector3.one * markerSize,
            new Vector3(-w * 0.5f, 0, 0), magenta);
        Box("ScreenMid_Right", o, r, Vector3.one * markerSize,
            new Vector3(w * 0.5f, 0, 0), magenta);

        // Screen plane border frame (thin boxes along edges)
        float frameThick = markerSize * 0.3f;
        Box("ScreenFrame_Top", o, r,
            new Vector3(w, frameThick, frameThick),
            new Vector3(0, h * 0.5f, 0), white);
        Box("ScreenFrame_Bottom", o, r,
            new Vector3(w, frameThick, frameThick),
            new Vector3(0, -h * 0.5f, 0), white);
        Box("ScreenFrame_Left", o, r,
            new Vector3(frameThick, h, frameThick),
            new Vector3(-w * 0.5f, 0, 0), white);
        Box("ScreenFrame_Right", o, r,
            new Vector3(frameThick, h, frameThick),
            new Vector3(w * 0.5f, 0, 0), white);

        // =============================================================
        // 2) BEHIND SCREEN — POSITIVE PARALLAX
        //    Objects should shift opposite to eye movement.
        //    Placed at z = 1w, 2w, 4w behind the screen.
        // =============================================================

        // --- Depth layer 1: z = 1w ---
        float z1 = w * 1f;

        // Row of pillars
        for (int i = -2; i <= 2; i++)
        {
            float px = i * w * 0.3f;
            float pillarH = h * 0.8f;
            Box($"Pillar_z1_{i}", o, r,
                new Vector3(w * 0.06f, pillarH, w * 0.06f),
                new Vector3(px, -h * 0.5f + pillarH * 0.5f, z1), gray);

            // Colored cap on each pillar for identification
            Box($"PillarCap_z1_{i}", o, r,
                new Vector3(w * 0.08f, markerSize, w * 0.08f),
                new Vector3(px, -h * 0.5f + pillarH, z1),
                i == 0 ? white : (i < 0 ? red : green));
        }

        // Depth label: sphere cluster spelling "1W"
        Sphere("Depth1_Marker", o, r, markerSize * 3,
            new Vector3(0, h * 0.3f, z1), orange);

        // --- Depth layer 2: z = 2w ---
        float z2 = w * 2f;

        // Archway made of cubes
        float archW = w * 0.5f;
        float archH = h * 0.7f;
        float archThick = w * 0.05f;
        Box("Arch_Left", o, r,
            new Vector3(archThick, archH, archThick),
            new Vector3(-archW * 0.5f, -h * 0.5f + archH * 0.5f, z2), blue);
        Box("Arch_Right", o, r,
            new Vector3(archThick, archH, archThick),
            new Vector3(archW * 0.5f, -h * 0.5f + archH * 0.5f, z2), blue);
        Box("Arch_Top", o, r,
            new Vector3(archW + archThick, archThick, archThick),
            new Vector3(0, -h * 0.5f + archH, z2), blue);

        Sphere("Depth2_Marker", o, r, markerSize * 3,
            new Vector3(0, h * 0.3f, z2), cyan);

        // --- Depth layer 3: z = 4w ---
        float z3 = w * 4f;

        // Large back wall with checkerboard pattern
        int tilesX = 8;
        int tilesY = 6;
        float tileW = w * 2f / tilesX;
        float tileH = h * 1.5f / tilesY;
        for (int ty = 0; ty < tilesY; ty++)
        {
            for (int tx = 0; tx < tilesX; tx++)
            {
                bool dark = (tx + ty) % 2 == 0;
                float tileX = -w + tileW * 0.5f + tx * tileW;
                float tileY = -h * 0.75f + tileH * 0.5f + ty * tileH;
                Box($"BackWallTile_{tx}_{ty}", o, r,
                    new Vector3(tileW * 0.98f, tileH * 0.98f, w * 0.02f),
                    new Vector3(tileX, tileY, z3),
                    dark ? darkGray : gray);
            }
        }

        Sphere("Depth3_Marker", o, r, markerSize * 4,
            new Vector3(0, h * 0.3f, z3), yellow);

        // =============================================================
        // 3) IN FRONT OF SCREEN — NEGATIVE PARALLAX
        //    Objects should shift WITH the eye movement (same direction).
        //    These "pop out" of the screen.
        // =============================================================

        float zFront1 = -w * 0.3f;
        float zFront2 = -w * 0.6f;

        // Floating sphere in front of screen
        Sphere("FrontSphere_1", o, r, markerSize * 4,
            new Vector3(-w * 0.2f, 0, zFront1), red);

        // Small cube further in front
        Box("FrontCube_1", o, r, Vector3.one * markerSize * 3,
            new Vector3(w * 0.2f, h * 0.1f, zFront2), magenta);

        // =============================================================
        // 4) GROUND PLANE WITH GRID — perspective correctness check
        //    Grid lines should stay straight. Convergence to vanishing
        //    point should look correct from any eye position.
        // =============================================================

        float floorY = -h * 0.5f;
        float gridExtent = w * 5f;
        float gridSpacing = w * 0.5f;
        float lineThick = markerSize * 0.15f;

        // Floor surface
        Box("Floor", o, r,
            new Vector3(gridExtent * 2f, lineThick, gridExtent),
            new Vector3(0, floorY - lineThick, gridExtent * 0.5f),
            new Color(0.15f, 0.15f, 0.15f));

        // Grid lines along Z (depth)
        int gridLines = Mathf.CeilToInt(gridExtent / gridSpacing);
        for (int i = -gridLines; i <= gridLines; i++)
        {
            float gx = i * gridSpacing;
            Box($"GridZ_{i}", o, r,
                new Vector3(lineThick * 2f, lineThick * 3f, gridExtent),
                new Vector3(gx, floorY, gridExtent * 0.5f),
                i == 0 ? green : new Color(0.3f, 0.3f, 0.3f));
        }

        // Grid lines along X (lateral)
        int depthLines = Mathf.CeilToInt(gridExtent / gridSpacing);
        for (int i = 0; i <= depthLines; i++)
        {
            float gz = i * gridSpacing;
            Box($"GridX_{i}", o, r,
                new Vector3(gridExtent * 2f, lineThick * 3f, lineThick * 2f),
                new Vector3(0, floorY, gz),
                i == 0 ? red : new Color(0.3f, 0.3f, 0.3f));
        }

        // =============================================================
        // 5) REFERENCE OBJECTS — distinctive shapes at known positions
        //    Easy to visually identify and track.
        // =============================================================

        // Three-axis indicator at center behind screen (z = 0.5w)
        float axLen = w * 0.15f;
        float axThick = markerSize * 0.5f;
        float axZ = w * 0.5f;
        Box("Axis_X", o, r,
            new Vector3(axLen, axThick, axThick),
            new Vector3(axLen * 0.5f, 0, axZ), red);
        Box("Axis_Y", o, r,
            new Vector3(axThick, axLen, axThick),
            new Vector3(0, axLen * 0.5f, axZ), green);
        Box("Axis_Z", o, r,
            new Vector3(axThick, axThick, axLen),
            new Vector3(0, 0, axZ + axLen * 0.5f), blue);

        // Sphere ladder — spheres at increasing heights and depths
        // Good for checking vertical + depth parallax simultaneously
        for (int i = 0; i < 5; i++)
        {
            float sz = w * 0.5f + i * w * 0.6f;
            float sy = -h * 0.3f + i * h * 0.15f;
            Sphere($"Ladder_{i}", o, r, markerSize * 2,
                new Vector3(w * 0.35f, sy, sz),
                Color.Lerp(green, blue, i / 4f));
        }

        // Cube tunnel — cubes getting smaller with depth (size constancy test)
        for (int i = 0; i < 6; i++)
        {
            float tz = w * 0.5f + i * w * 0.7f;
            float tScale = w * 0.15f - i * w * 0.015f;
            // Only outline: four vertical edges
            float edgeThick = markerSize * 0.3f;
            Box($"Tunnel_TL_{i}", o, r,
                new Vector3(edgeThick, tScale, edgeThick),
                new Vector3(-tScale * 0.5f, tScale * 0.5f - h * 0.25f, tz), yellow);
            Box($"Tunnel_TR_{i}", o, r,
                new Vector3(edgeThick, tScale, edgeThick),
                new Vector3(tScale * 0.5f, tScale * 0.5f - h * 0.25f, tz), yellow);
            Box($"Tunnel_BL_{i}", o, r,
                new Vector3(edgeThick, edgeThick, edgeThick),
                new Vector3(-tScale * 0.5f, -h * 0.25f, tz), yellow);
            Box($"Tunnel_BR_{i}", o, r,
                new Vector3(edgeThick, edgeThick, edgeThick),
                new Vector3(tScale * 0.5f, -h * 0.25f, tz), yellow);
            // Top and bottom horizontal edges
            Box($"Tunnel_Top_{i}", o, r,
                new Vector3(tScale, edgeThick, edgeThick),
                new Vector3(0, tScale - h * 0.25f, tz), yellow);
            Box($"Tunnel_Bot_{i}", o, r,
                new Vector3(tScale, edgeThick, edgeThick),
                new Vector3(0, -h * 0.25f, tz), yellow);
        }

        // =============================================================
        // 6) LIGHTING
        // =============================================================
        var dirLight = new GameObject("TestScene_DirectionalLight");
        dirLight.transform.parent = _root.transform;
        dirLight.transform.position = o + r * new Vector3(0, h, -w);
        dirLight.transform.rotation = r * Quaternion.Euler(45, 30, 0);
        var light = dirLight.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(0.95f, 0.92f, 0.88f);
        light.intensity = 1f;
        light.shadows = LightShadows.Soft;

        Debug.Log("[ProjectionTestScene] Generated. Move the eye position to test:\n" +
                  "- Screen-plane markers (colored corners) should NOT move\n" +
                  "- Behind-screen objects should shift OPPOSITE to eye\n" +
                  "- In-front objects should shift WITH the eye\n" +
                  "- Grid lines should stay straight");
    }

    // --- Helpers ---

    void Box(string name, Vector3 origin, Quaternion rot, Vector3 scale, Vector3 local, Color c)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Setup(go, name, origin, rot, local);
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().material.color = c;
    }

    void Sphere(string name, Vector3 origin, Quaternion rot, float size, Vector3 local, Color c)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Setup(go, name, origin, rot, local);
        go.transform.localScale = Vector3.one * size;
        go.GetComponent<Renderer>().material.color = c;
    }

    void Setup(GameObject go, string name, Vector3 origin, Quaternion rot, Vector3 local)
    {
        go.name = name;
        go.transform.parent = _root.transform;
        go.transform.position = origin + rot * local;
        go.transform.rotation = rot;
    }

    [ContextMenu("Clear Scene")]
    public void Clear()
    {
        if (_root != null) DestroyImmediate(_root);
    }
}
