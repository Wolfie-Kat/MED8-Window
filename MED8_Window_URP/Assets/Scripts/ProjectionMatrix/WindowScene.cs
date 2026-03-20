using UnityEngine;

/// <summary>
/// Looking through a thick stone window into a cozy room.
/// The thick wall around the window opening is the strongest
/// depth cue for the virtual window illusion — as you move your head,
/// the wall edges occlude different parts of the room behind it.
/// </summary>
public class WindowScene : MonoBehaviour
{
    [Header("References")]
    public ProjectionPlane Screen;

    [Header("Settings")]
    public bool GenerateOnStart = true;

    private GameObject _root;

    void Start()
    {
        if (GenerateOnStart && Screen != null)
            Generate();
    }

    [ContextMenu("Generate Window Scene")]
    public void Generate()
    {
        if (_root != null) DestroyImmediate(_root);
        _root = new GameObject("--- WINDOW SCENE ---");

        float w = Screen.Size.x;
        float h = Screen.Size.y;
        Vector3 o = Screen.transform.position;
        Quaternion r = Screen.transform.rotation;

        float floorY = -h * 0.5f;
        float ceilY = h * 0.5f;
        float roomW = w * 2.5f;
        float roomH = h * 1.2f;
        float roomD = w * 6f;

        // Colors
        Color stoneOuter = new Color(0.40f, 0.37f, 0.33f);
        Color stoneInner = new Color(0.48f, 0.44f, 0.40f);
        Color wallColor = new Color(0.55f, 0.50f, 0.42f);
        Color floorWood = new Color(0.35f, 0.24f, 0.14f);
        Color darkWood = new Color(0.22f, 0.14f, 0.08f);
        Color warmWood = new Color(0.40f, 0.26f, 0.14f);
        Color gold = new Color(0.80f, 0.60f, 0.15f);
        Color red = new Color(0.60f, 0.12f, 0.08f);
        Color green = new Color(0.15f, 0.35f, 0.12f);
        Color cream = new Color(0.85f, 0.80f, 0.70f);
        Color fireOrange = new Color(1f, 0.55f, 0.1f);
        Color fireYellow = new Color(1f, 0.85f, 0.3f);
        Color ceilColor = new Color(0.45f, 0.42f, 0.38f);
        Color rugColor = new Color(0.50f, 0.15f, 0.10f);
        Color rugBorder = new Color(0.55f, 0.40f, 0.10f);

        // =================================================================
        // THE WALL — thick wall surrounding the window opening
        // This is the KEY element. It must be thick (deep in Z) so the
        // edges are visible and occlude as you move your head.
        // Everything here is at/near Z=0 (screen plane) = zero parallax.
        // =================================================================
        float wallThickness = w * 0.15f;
        float wallExtend = w * 0.8f;

        // Wall above window
        Box("Wall_Top", o, r,
            new Vector3(w + wallExtend * 2f, wallExtend, wallThickness),
            new Vector3(0, ceilY + wallExtend * 0.5f, wallThickness * 0.5f), stoneOuter);

        // Wall below window
        Box("Wall_Bottom", o, r,
            new Vector3(w + wallExtend * 2f, wallExtend, wallThickness),
            new Vector3(0, floorY - wallExtend * 0.5f, wallThickness * 0.5f), stoneOuter);

        // Wall left of window
        Box("Wall_Left", o, r,
            new Vector3(wallExtend, h + wallExtend * 2f, wallThickness),
            new Vector3(-w * 0.5f - wallExtend * 0.5f, 0, wallThickness * 0.5f), stoneOuter);

        // Wall right of window
        Box("Wall_Right", o, r,
            new Vector3(wallExtend, h + wallExtend * 2f, wallThickness),
            new Vector3(w * 0.5f + wallExtend * 0.5f, 0, wallThickness * 0.5f), stoneOuter);

        // Inner beveled edges of the window opening (the "depth" you see)
        // Top edge
        Box("Bevel_Top", o, r,
            new Vector3(w, h * 0.03f, wallThickness),
            new Vector3(0, ceilY - h * 0.015f, wallThickness * 0.5f), stoneInner);
        // Bottom edge (window sill)
        Box("Bevel_Bottom", o, r,
            new Vector3(w * 1.05f, h * 0.05f, wallThickness * 1.1f),
            new Vector3(0, floorY + h * 0.025f, wallThickness * 0.5f), stoneInner);
        // Left edge
        Box("Bevel_Left", o, r,
            new Vector3(w * 0.03f, h, wallThickness),
            new Vector3(-w * 0.5f + w * 0.015f, 0, wallThickness * 0.5f), stoneInner);
        // Right edge
        Box("Bevel_Right", o, r,
            new Vector3(w * 0.03f, h, wallThickness),
            new Vector3(w * 0.5f - w * 0.015f, 0, wallThickness * 0.5f), stoneInner);

        // Small object on the window sill (at screen plane)
        // Potted plant
        Box("Pot", o, r,
            new Vector3(w * 0.04f, h * 0.05f, w * 0.04f),
            new Vector3(w * 0.3f, floorY + h * 0.075f, wallThickness * 0.3f),
            new Color(0.55f, 0.30f, 0.15f));
        Sphere("Plant_1", o, r, w * 0.03f,
            new Vector3(w * 0.3f, floorY + h * 0.12f, wallThickness * 0.3f), green);
        Sphere("Plant_2", o, r, w * 0.025f,
            new Vector3(w * 0.32f, floorY + h * 0.14f, wallThickness * 0.28f), new Color(0.18f, 0.40f, 0.14f));
        Sphere("Plant_3", o, r, w * 0.025f,
            new Vector3(w * 0.28f, floorY + h * 0.13f, wallThickness * 0.32f), new Color(0.12f, 0.32f, 0.10f));

        // Extra items on the window sill (at screen plane — zero parallax, very visible)
        // Lantern on left side of sill
        Box("Lantern_Base", o, r,
            new Vector3(w * 0.035f, h * 0.005f, w * 0.035f),
            new Vector3(-w * 0.3f, floorY + h * 0.055f, wallThickness * 0.4f), darkWood);
        Box("Lantern_Body", o, r,
            new Vector3(w * 0.025f, h * 0.07f, w * 0.025f),
            new Vector3(-w * 0.3f, floorY + h * 0.095f, wallThickness * 0.4f), new Color(0.2f, 0.2f, 0.2f));
        Sphere("Lantern_Glow", o, r, w * 0.015f,
            new Vector3(-w * 0.3f, floorY + h * 0.095f, wallThickness * 0.4f), fireYellow);
        PointLight("LanternLight", o, r,
            new Vector3(-w * 0.3f, floorY + h * 0.1f, wallThickness * 0.4f),
            new Color(1f, 0.8f, 0.4f), w * 0.3f, 0.4f);

        // Small stack of books on the sill
        Box("SillBook_1", o, r,
            new Vector3(w * 0.04f, h * 0.012f, w * 0.03f),
            new Vector3(-w * 0.1f, floorY + h * 0.056f, wallThickness * 0.35f), red);
        Box("SillBook_2", o, r,
            new Vector3(w * 0.038f, h * 0.01f, w * 0.032f),
            new Vector3(-w * 0.1f, floorY + h * 0.067f, wallThickness * 0.35f), new Color(0.2f, 0.2f, 0.5f));

        // Mug on the sill
        Cylinder("Mug", o, r,
            new Vector3(w * 0.018f, h * 0.03f, w * 0.018f),
            new Vector3(w * 0.1f, floorY + h * 0.065f, wallThickness * 0.35f),
            Quaternion.identity, cream);

        // Cat sitting on the sill (right side)
        Sphere("CatBody", o, r, w * 0.035f,
            new Vector3(w * 0.35f, floorY + h * 0.085f, wallThickness * 0.3f), new Color(0.15f, 0.12f, 0.10f));
        Sphere("CatHead", o, r, w * 0.022f,
            new Vector3(w * 0.35f, floorY + h * 0.12f, wallThickness * 0.22f), new Color(0.15f, 0.12f, 0.10f));
        // Cat ears
        Box("CatEar_L", o, r,
            new Vector3(w * 0.006f, h * 0.015f, w * 0.005f),
            new Vector3(w * 0.34f, floorY + h * 0.14f, wallThickness * 0.2f), new Color(0.15f, 0.12f, 0.10f));
        Box("CatEar_R", o, r,
            new Vector3(w * 0.006f, h * 0.015f, w * 0.005f),
            new Vector3(w * 0.36f, floorY + h * 0.14f, wallThickness * 0.2f), new Color(0.15f, 0.12f, 0.10f));
        // Cat tail
        Cylinder("CatTail", o, r,
            new Vector3(w * 0.005f, w * 0.04f, w * 0.005f),
            new Vector3(w * 0.38f, floorY + h * 0.075f, wallThickness * 0.35f),
            Quaternion.Euler(0, 0, 70), new Color(0.15f, 0.12f, 0.10f));

        // =================================================================
        // NEAR LAYER (just behind the wall, 0.15w - 1.5w)
        // Subtle parallax. First things you see through the window.
        // =================================================================
        float nearZ = wallThickness + w * 0.1f;

        // Coat rack / hat stand near the window
        Cylinder("CoatRack", o, r,
            new Vector3(w * 0.012f, h * 0.55f, w * 0.012f),
            new Vector3(-w * 0.35f, floorY + h * 0.275f, nearZ),
            Quaternion.identity, darkWood);
        // Hat on the rack
        Cylinder("Hat", o, r,
            new Vector3(w * 0.05f, h * 0.015f, w * 0.05f),
            new Vector3(-w * 0.35f, floorY + h * 0.55f, nearZ),
            Quaternion.identity, new Color(0.25f, 0.18f, 0.12f));
        // Coat draped
        Box("Coat", o, r,
            new Vector3(w * 0.06f, h * 0.2f, w * 0.02f),
            new Vector3(-w * 0.35f, floorY + h * 0.38f, nearZ + w * 0.01f), new Color(0.20f, 0.22f, 0.30f));

        // Side table with candle
        float tableZ = nearZ + w * 0.3f;
        Box("SideTable", o, r,
            new Vector3(w * 0.12f, h * 0.02f, w * 0.08f),
            new Vector3(w * 0.35f, floorY + h * 0.28f, tableZ), warmWood);
        // Table legs
        for (int leg = 0; leg < 4; leg++)
        {
            float lx = w * 0.35f + (leg % 2 == 0 ? -1 : 1) * w * 0.045f;
            float lz = tableZ + (leg < 2 ? -1 : 1) * w * 0.03f;
            Box($"SideTableLeg_{leg}", o, r,
                new Vector3(w * 0.012f, h * 0.27f, w * 0.012f),
                new Vector3(lx, floorY + h * 0.135f, lz), darkWood);
        }
        // Candle on table
        Cylinder("TableCandle", o, r,
            new Vector3(w * 0.01f, h * 0.06f, w * 0.01f),
            new Vector3(w * 0.35f, floorY + h * 0.32f, tableZ),
            Quaternion.identity, cream);
        Sphere("TableFlame", o, r, w * 0.012f,
            new Vector3(w * 0.35f, floorY + h * 0.36f, tableZ), fireYellow);
        PointLight("TableCandleLight", o, r,
            new Vector3(w * 0.35f, floorY + h * 0.36f, tableZ),
            new Color(1f, 0.8f, 0.4f), w * 0.5f, 0.6f);

        // =================================================================
        // MID LAYER (1.5w - 3.5w) — strong parallax
        // The main room contents.
        // =================================================================
        float midZ = wallThickness + w * 1.5f;

        // Large dining table in center
        float dTableW = w * 0.5f;
        float dTableD = w * 0.25f;
        float dTableY = floorY + h * 0.32f;
        Box("DiningTable", o, r,
            new Vector3(dTableW, h * 0.025f, dTableD),
            new Vector3(0, dTableY, midZ), warmWood);
        // Table legs
        for (int leg = 0; leg < 4; leg++)
        {
            float lx = (leg % 2 == 0 ? -1 : 1) * dTableW * 0.42f;
            float lz = midZ + (leg < 2 ? -1 : 1) * dTableD * 0.38f;
            Box($"DTableLeg_{leg}", o, r,
                new Vector3(w * 0.02f, h * 0.3f, w * 0.02f),
                new Vector3(lx, floorY + h * 0.15f, lz), darkWood);
        }

        // Items on the table
        // Plate
        Cylinder("Plate", o, r,
            new Vector3(w * 0.06f, h * 0.005f, w * 0.06f),
            new Vector3(-w * 0.1f, dTableY + h * 0.015f, midZ),
            Quaternion.identity, cream);
        // Bowl
        Cylinder("Bowl", o, r,
            new Vector3(w * 0.04f, h * 0.02f, w * 0.04f),
            new Vector3(w * 0.08f, dTableY + h * 0.02f, midZ - w * 0.05f),
            Quaternion.identity, cream);
        // Wine bottle
        Cylinder("Bottle", o, r,
            new Vector3(w * 0.012f, h * 0.1f, w * 0.012f),
            new Vector3(w * 0.02f, dTableY + h * 0.06f, midZ + w * 0.05f),
            Quaternion.identity, new Color(0.15f, 0.30f, 0.12f));
        // Goblet
        Cylinder("Goblet", o, r,
            new Vector3(w * 0.012f, h * 0.04f, w * 0.012f),
            new Vector3(-w * 0.05f, dTableY + h * 0.03f, midZ + w * 0.03f),
            Quaternion.identity, gold);
        // Bread
        Sphere("Bread", o, r, w * 0.03f,
            new Vector3(w * 0.15f, dTableY + h * 0.025f, midZ), new Color(0.70f, 0.55f, 0.30f));
        // Candelabra centerpiece
        Cylinder("Candelabra", o, r,
            new Vector3(w * 0.008f, h * 0.1f, w * 0.008f),
            new Vector3(0, dTableY + h * 0.06f, midZ),
            Quaternion.identity, gold);
        Sphere("CFlame_1", o, r, w * 0.01f,
            new Vector3(-w * 0.02f, dTableY + h * 0.12f, midZ), fireYellow);
        Sphere("CFlame_2", o, r, w * 0.01f,
            new Vector3(w * 0.02f, dTableY + h * 0.12f, midZ), fireYellow);
        Sphere("CFlame_3", o, r, w * 0.01f,
            new Vector3(0, dTableY + h * 0.13f, midZ), fireOrange);
        PointLight("CandelabraLight", o, r,
            new Vector3(0, dTableY + h * 0.13f, midZ),
            new Color(1f, 0.85f, 0.5f), w * 1.2f, 1f);

        // Chairs around table
        float chairH = h * 0.25f;
        float chairSeat = floorY + chairH;
        float[] chairXs = { -dTableW * 0.35f, dTableW * 0.35f };
        float[] chairZs = { midZ - dTableD * 0.6f, midZ + dTableD * 0.6f };
        int ci = 0;
        foreach (float cx in chairXs)
        {
            foreach (float cz in chairZs)
            {
                Box($"ChairSeat_{ci}", o, r,
                    new Vector3(w * 0.07f, h * 0.015f, w * 0.06f),
                    new Vector3(cx, chairSeat, cz), darkWood);
                // Back rest (on the far side)
                Box($"ChairBack_{ci}", o, r,
                    new Vector3(w * 0.07f, h * 0.2f, w * 0.01f),
                    new Vector3(cx, chairSeat + h * 0.1f, cz + w * 0.025f), darkWood);
                // Chair legs
                for (int cl = 0; cl < 4; cl++)
                {
                    float clx = cx + (cl % 2 == 0 ? -1 : 1) * w * 0.025f;
                    float clz = cz + (cl < 2 ? -1 : 1) * w * 0.02f;
                    Box($"ChairLeg_{ci}_{cl}", o, r,
                        new Vector3(w * 0.008f, chairH, w * 0.008f),
                        new Vector3(clx, floorY + chairH * 0.5f, clz), darkWood);
                }
                ci++;
            }
        }

        // Rug under the table
        Box("Rug", o, r,
            new Vector3(dTableW * 1.4f, 0.06f, dTableD * 1.8f),
            new Vector3(0, floorY, midZ), rugColor);
        Box("RugBorder", o, r,
            new Vector3(dTableW * 1.5f, 0.055f, dTableD * 1.9f),
            new Vector3(0, floorY, midZ), rugBorder);

        // Bookshelf on left wall
        float shelfZ = midZ + w * 0.5f;
        float shelfX = -roomW * 0.48f;
        Box("Bookshelf", o, r,
            new Vector3(w * 0.06f, h * 0.7f, w * 0.2f),
            new Vector3(shelfX, floorY + h * 0.35f, shelfZ), darkWood);
        // Shelves
        for (int s = 0; s < 4; s++)
        {
            Box($"Shelf_{s}", o, r,
                new Vector3(w * 0.065f, h * 0.01f, w * 0.2f),
                new Vector3(shelfX, floorY + h * 0.12f + s * h * 0.17f, shelfZ), warmWood);
        }
        // Books on shelves
        Color[] bookColors = { red, green, new Color(0.2f, 0.2f, 0.5f), gold, darkWood, cream };
        for (int s = 0; s < 4; s++)
        {
            int books = Random.Range(4, 8);
            float bookZ = shelfZ - w * 0.08f;
            for (int b = 0; b < books; b++)
            {
                float bh = h * Random.Range(0.08f, 0.14f);
                float by = floorY + h * 0.13f + s * h * 0.17f + bh * 0.5f;
                Box($"Book_{s}_{b}", o, r,
                    new Vector3(w * 0.008f, bh, w * Random.Range(0.03f, 0.05f)),
                    new Vector3(shelfX + w * 0.005f, by, bookZ + b * w * 0.015f),
                    bookColors[Random.Range(0, bookColors.Length)]);
            }
        }

        // =================================================================
        // FAR LAYER (3.5w - 6w) — back wall with fireplace
        // Maximum parallax. The anchor of the room.
        // =================================================================
        float farZ = roomD;

        // Back wall
        Box("BackWall", o, r,
            new Vector3(roomW, roomH * 1.2f, w * 0.08f),
            new Vector3(0, floorY + roomH * 0.5f, farZ), wallColor);

        // Fireplace
        float fpW = w * 0.4f;
        float fpH = h * 0.4f;
        float fpZ = farZ - w * 0.04f;
        // Mantle
        Box("Mantle", o, r,
            new Vector3(fpW * 1.3f, h * 0.04f, w * 0.1f),
            new Vector3(0, floorY + fpH + h * 0.02f, fpZ), stoneInner);
        // Fireplace sides
        Box("FP_Left", o, r,
            new Vector3(w * 0.06f, fpH, w * 0.08f),
            new Vector3(-fpW * 0.5f, floorY + fpH * 0.5f, fpZ), stoneOuter);
        Box("FP_Right", o, r,
            new Vector3(w * 0.06f, fpH, w * 0.08f),
            new Vector3(fpW * 0.5f, floorY + fpH * 0.5f, fpZ), stoneOuter);
        // Firebox (dark interior)
        Box("Firebox", o, r,
            new Vector3(fpW - w * 0.12f, fpH * 0.85f, w * 0.06f),
            new Vector3(0, floorY + fpH * 0.42f, fpZ + w * 0.01f), new Color(0.08f, 0.06f, 0.05f));
        // Fire logs
        Cylinder("Log_1", o, r,
            new Vector3(w * 0.02f, fpW * 0.3f, w * 0.02f),
            new Vector3(-w * 0.03f, floorY + h * 0.04f, fpZ - w * 0.01f),
            Quaternion.Euler(0, 15, 85), darkWood);
        Cylinder("Log_2", o, r,
            new Vector3(w * 0.018f, fpW * 0.25f, w * 0.018f),
            new Vector3(w * 0.02f, floorY + h * 0.05f, fpZ - w * 0.005f),
            Quaternion.Euler(0, -20, 80), darkWood);
        // Flames
        Sphere("Fire_1", o, r, w * 0.04f,
            new Vector3(0, floorY + h * 0.1f, fpZ - w * 0.01f), fireOrange);
        Sphere("Fire_2", o, r, w * 0.03f,
            new Vector3(-w * 0.03f, floorY + h * 0.12f, fpZ - w * 0.005f), fireYellow);
        Sphere("Fire_3", o, r, w * 0.025f,
            new Vector3(w * 0.02f, floorY + h * 0.14f, fpZ - w * 0.008f), fireYellow);
        // Firelight
        PointLight("FireLight", o, r,
            new Vector3(0, floorY + h * 0.15f, fpZ - w * 0.05f),
            new Color(1f, 0.6f, 0.2f), w * 2f, 1.5f);
        PointLight("FireGlow", o, r,
            new Vector3(0, floorY + h * 0.05f, fpZ - w * 0.1f),
            new Color(1f, 0.4f, 0.1f), w * 1f, 0.6f);

        // Items on mantle
        // Clock
        Box("Clock", o, r,
            new Vector3(w * 0.04f, h * 0.06f, w * 0.025f),
            new Vector3(0, floorY + fpH + h * 0.07f, fpZ - w * 0.01f), darkWood);
        Sphere("ClockFace", o, r, w * 0.015f,
            new Vector3(0, floorY + fpH + h * 0.075f, fpZ - w * 0.025f), cream);
        // Candlesticks on mantle
        Cylinder("MantleCandle_L", o, r,
            new Vector3(w * 0.008f, h * 0.07f, w * 0.008f),
            new Vector3(-fpW * 0.4f, floorY + fpH + h * 0.055f, fpZ - w * 0.01f),
            Quaternion.identity, gold);
        Sphere("MantleFlame_L", o, r, w * 0.01f,
            new Vector3(-fpW * 0.4f, floorY + fpH + h * 0.095f, fpZ - w * 0.01f), fireYellow);
        Cylinder("MantleCandle_R", o, r,
            new Vector3(w * 0.008f, h * 0.07f, w * 0.008f),
            new Vector3(fpW * 0.4f, floorY + fpH + h * 0.055f, fpZ - w * 0.01f),
            Quaternion.identity, gold);
        Sphere("MantleFlame_R", o, r, w * 0.01f,
            new Vector3(fpW * 0.4f, floorY + fpH + h * 0.095f, fpZ - w * 0.01f), fireYellow);

        // Painting above fireplace
        Box("PaintingFrame", o, r,
            new Vector3(w * 0.3f, h * 0.22f, w * 0.015f),
            new Vector3(0, floorY + fpH + h * 0.25f, fpZ - w * 0.005f), gold);
        Box("PaintingCanvas", o, r,
            new Vector3(w * 0.27f, h * 0.19f, w * 0.01f),
            new Vector3(0, floorY + fpH + h * 0.25f, fpZ - w * 0.01f), new Color(0.3f, 0.4f, 0.35f));

        // Armchair by the fireplace
        float armZ = farZ - w * 0.8f;
        Box("ArmchairSeat", o, r,
            new Vector3(w * 0.12f, h * 0.04f, w * 0.1f),
            new Vector3(w * 0.25f, floorY + h * 0.2f, armZ), red);
        Box("ArmchairBack", o, r,
            new Vector3(w * 0.12f, h * 0.2f, w * 0.03f),
            new Vector3(w * 0.25f, floorY + h * 0.32f, armZ + w * 0.04f), red);
        Box("ArmchairArm_L", o, r,
            new Vector3(w * 0.02f, h * 0.1f, w * 0.1f),
            new Vector3(w * 0.25f - w * 0.06f, floorY + h * 0.24f, armZ), red);
        Box("ArmchairArm_R", o, r,
            new Vector3(w * 0.02f, h * 0.1f, w * 0.1f),
            new Vector3(w * 0.25f + w * 0.06f, floorY + h * 0.24f, armZ), red);

        // =================================================================
        // ROOM ENCLOSURE — floor, ceiling, side walls
        // =================================================================

        // Wooden floor
        Box("Floor", o, r,
            new Vector3(roomW, 0.05f, roomD),
            new Vector3(0, floorY, wallThickness + roomD * 0.5f), floorWood);

        // Ceiling with beams
        Box("Ceiling", o, r,
            new Vector3(roomW, 0.08f, roomD),
            new Vector3(0, floorY + roomH, wallThickness + roomD * 0.5f), ceilColor);
        for (int i = 0; i < 6; i++)
        {
            float bz = wallThickness + (roomD / 6f) * (i + 0.5f);
            Box($"Beam_{i}", o, r,
                new Vector3(roomW, h * 0.05f, w * 0.05f),
                new Vector3(0, floorY + roomH - h * 0.025f, bz), darkWood);
        }

        // Left wall
        Box("Wall_L", o, r,
            new Vector3(0.08f, roomH, roomD),
            new Vector3(-roomW * 0.5f, floorY + roomH * 0.5f, wallThickness + roomD * 0.5f), wallColor);

        // Right wall
        Box("Wall_R", o, r,
            new Vector3(0.08f, roomH, roomD),
            new Vector3(roomW * 0.5f, floorY + roomH * 0.5f, wallThickness + roomD * 0.5f), wallColor);

        // Baseboards
        Box("Baseboard_L", o, r,
            new Vector3(w * 0.03f, h * 0.04f, roomD),
            new Vector3(-roomW * 0.5f + w * 0.015f, floorY + h * 0.02f, wallThickness + roomD * 0.5f), darkWood);
        Box("Baseboard_R", o, r,
            new Vector3(w * 0.03f, h * 0.04f, roomD),
            new Vector3(roomW * 0.5f - w * 0.015f, floorY + h * 0.02f, wallThickness + roomD * 0.5f), darkWood);
        Box("Baseboard_Back", o, r,
            new Vector3(roomW, h * 0.04f, w * 0.03f),
            new Vector3(0, floorY + h * 0.02f, farZ - w * 0.015f), darkWood);

        // Crown molding at ceiling
        Box("Crown_L", o, r,
            new Vector3(w * 0.025f, h * 0.025f, roomD),
            new Vector3(-roomW * 0.5f + w * 0.012f, floorY + roomH - h * 0.012f, wallThickness + roomD * 0.5f), stoneInner);
        Box("Crown_R", o, r,
            new Vector3(w * 0.025f, h * 0.025f, roomD),
            new Vector3(roomW * 0.5f - w * 0.012f, floorY + roomH - h * 0.012f, wallThickness + roomD * 0.5f), stoneInner);

        // =================================================================
        // LIGHTING
        // =================================================================
        // Warm ambient from the room
        var ambGo = new GameObject("AmbientLight");
        ambGo.transform.parent = _root.transform;
        ambGo.transform.position = o;
        ambGo.transform.rotation = r * Quaternion.Euler(50, 0, 0);
        var ambLight = ambGo.AddComponent<Light>();
        ambLight.type = LightType.Directional;
        ambLight.color = new Color(0.6f, 0.5f, 0.4f);
        ambLight.intensity = 0.2f;
        ambLight.shadows = LightShadows.Soft;

        Debug.Log("Window scene generated: thick-wall window looking into a furnished room with fireplace.");
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

    void Cylinder(string name, Vector3 origin, Quaternion rot, Vector3 scale, Vector3 local, Quaternion localRot, Color c)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Setup(go, name, origin, rot, local);
        go.transform.rotation = rot * localRot;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().material.color = c;
    }

    void PointLight(string name, Vector3 origin, Quaternion rot, Vector3 local, Color c, float range, float intensity)
    {
        var go = new GameObject(name);
        go.transform.parent = _root.transform;
        go.transform.position = origin + rot * local;
        var light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = c;
        light.range = range;
        light.intensity = intensity;
        light.shadows = LightShadows.Soft;
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
