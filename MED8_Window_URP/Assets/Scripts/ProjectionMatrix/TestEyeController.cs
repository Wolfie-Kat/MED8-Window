using UnityEngine;

/// <summary>
/// Keyboard-controlled eye position for testing off-axis projection
/// without head tracking hardware.
///
/// Controls:
///   A/D — move eye left/right
///   W/S — move eye forward/backward (toward/away from screen)
///   Q/E — move eye down/up
///   R   — reset to start position
///   1-3 — jump to preset positions (center, left-offset, right-offset)
///
/// Attach to the HeadPosition transform that ProjectionPlaneCamera reads.
/// </summary>
public class TestEyeController : MonoBehaviour
{
    [Header("Movement")]
    public float MoveSpeed = 2f;
    public float FastMultiplier = 3f;

    [Header("Limits")]
    public float MaxOffsetX = 5f;
    public float MaxOffsetY = 3f;
    public float MinDistanceZ = 0.5f;
    public float MaxDistanceZ = 10f;

    private Vector3 _startPos;

    void Start()
    {
        _startPos = transform.position;
    }

    void Update()
    {
        float speed = MoveSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            speed *= FastMultiplier;

        float dt = Time.deltaTime * speed;

        Vector3 pos = transform.position;

        // Lateral (left/right)
        if (Input.GetKey(KeyCode.A)) pos.x -= dt;
        if (Input.GetKey(KeyCode.D)) pos.x += dt;

        // Vertical (up/down)
        if (Input.GetKey(KeyCode.E)) pos.y += dt;
        if (Input.GetKey(KeyCode.Q)) pos.y -= dt;

        // Forward/backward (toward/away from screen)
        if (Input.GetKey(KeyCode.W)) pos.z -= dt;
        if (Input.GetKey(KeyCode.S)) pos.z += dt;

        // Clamp
        pos.x = Mathf.Clamp(pos.x, _startPos.x - MaxOffsetX, _startPos.x + MaxOffsetX);
        pos.y = Mathf.Clamp(pos.y, _startPos.y - MaxOffsetY, _startPos.y + MaxOffsetY);
        pos.z = Mathf.Clamp(pos.z, _startPos.z - MaxDistanceZ, _startPos.z - MinDistanceZ);

        transform.position = pos;

        // Reset
        if (Input.GetKeyDown(KeyCode.R))
            transform.position = _startPos;

        // Presets
        if (Input.GetKeyDown(KeyCode.Alpha1))
            transform.position = _startPos; // Center

        if (Input.GetKeyDown(KeyCode.Alpha2))
            transform.position = _startPos + Vector3.left * 1.5f; // Left offset

        if (Input.GetKeyDown(KeyCode.Alpha3))
            transform.position = _startPos + Vector3.right * 1.5f; // Right offset
    }

    void OnGUI()
    {
        Vector3 offset = transform.position - _startPos;
        string info = $"Eye Offset: X={offset.x:F2}  Y={offset.y:F2}  Z={offset.z:F2}\n" +
                      $"[WASD] move  [QE] up/down  [R] reset  [1-3] presets  [Shift] fast";
        GUI.Label(new Rect(10, 10, 600, 50), info);
    }
}
