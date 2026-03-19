using UnityEngine;

public class BlindsShutterMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;

    [Header("Limits (Local Y positions)")]
    public float minY = 0f;   // fully down
    public float maxY = 2f;   // fully up

    private float targetY;


    void Start()
    {
        // Start at current position
        targetY = transform.localPosition.y;
    }

    // Update is called once per frame
    void Update()
    {
        float input = 0f;

        // TEST INPUT (replace later if needed)
        if (Input.GetKey(KeyCode.W))
            input = 1f;     // move up
        if (Input.GetKey(KeyCode.S))
            input = -1f;    // move down

        // Move target position
        targetY += input * speed * Time.deltaTime;

        // Clamp between limits
        targetY = Mathf.Clamp(targetY, minY, maxY);

        // Apply movement smoothly
        Vector3 pos = transform.localPosition;
        pos.y = targetY;
        transform.localPosition = pos;
    }
}
