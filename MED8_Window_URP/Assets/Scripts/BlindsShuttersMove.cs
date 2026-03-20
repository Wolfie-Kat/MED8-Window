using UnityEngine;

public class BlindsShutterMove : MonoBehaviour
{
    [Header("Assign TOP → BOTTOM")]
    public Transform[] slats;

    [Header("Movement")]
    public float liftSpeed = 0.6f;
    [Range(0f, 1f)]
    public float openAmount = 0f;

    float targetOpen;

    float[] startY;
    float[] slatHeights;

    [Header("Tilt")]
    public float tiltSpeed = 120f;
    [Range(0f,1f)]
    public float tiltAmount = 0.5f; // 0.5 ≈ -90°

    public float minTiltX = -160f;
    public float maxTiltX = -20f;

    float targetTilt;

    void Start()
    {
        int count = slats.Length;

        startY = new float[count];
        slatHeights = new float[count];
        targetTilt = tiltAmount;

        // Record starting positions
        for (int i = 0; i < count; i++)
        {
            startY[i] = slats[i].localPosition.y;

            // Automatically measure slat thickness
            Renderer r = slats[i].GetComponentInChildren<Renderer>();
            if (r != null)
                slatHeights[i] = r.bounds.size.y;
            else
                slatHeights[i] = 0.02f; // fallback
        }
    }

    void Update()
    {
        // TEST INPUT
        if (Input.GetKey(KeyCode.W))
            targetOpen += liftSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.S))
            targetOpen -= liftSpeed * Time.deltaTime;

        targetOpen = Mathf.Clamp01(targetOpen);

        openAmount = Mathf.MoveTowards(openAmount, targetOpen, liftSpeed * Time.deltaTime);

        // Tilt input
        if (Input.GetKey(KeyCode.D))
            targetTilt += tiltSpeed * Time.deltaTime / 180f;

        if (Input.GetKey(KeyCode.A))
            targetTilt -= tiltSpeed * Time.deltaTime / 180f;

        targetTilt = Mathf.Clamp01(targetTilt);

        tiltAmount = Mathf.MoveTowards(tiltAmount, targetTilt, tiltSpeed * Time.deltaTime / 180f);

        UpdateSlats();
        UpdateTilt();
    }

    void UpdateSlats()
    {
        int count = slats.Length;

        // stack starts at top slat original height
        float stackY = startY[0];

        for (int i = 0; i < count; i++)
        {
            float normalized = (float)i / (count - 1);

            // bottom moves more than top
            float weight = Mathf.Lerp(0.9f, 0.9f, normalized);

            // where slat WOULD like to go
            float desiredY = Mathf.Lerp(
                startY[i],
                startY[0],
                openAmount * weight
            );

            // TRUE STACK CONSTRAINT
            float finalY = Mathf.Min(desiredY, stackY);

            Vector3 pos = slats[i].localPosition;
            pos.y = finalY;
            slats[i].localPosition = pos;

            // next slat must sit below this one
            stackY = finalY - slatHeights[i];
        }
    }

    void UpdateTilt()
    {
            // how much tilt is allowed based on lift
        float tiltStrength = 1f - openAmount;

        // optional smoothing so tilt fades earlier
        tiltStrength = Mathf.SmoothStep(0f, 1f, tiltStrength);

        // base tilt from input
        float baseTiltX = Mathf.Lerp(minTiltX, maxTiltX, tiltAmount);

        // neutral angle (center between min & max)
        float neutralTilt = (minTiltX + maxTiltX) * 0.5f;

        // reduce tilt toward neutral as blinds lift
        float tiltX = Mathf.Lerp(neutralTilt, baseTiltX, tiltStrength);

        Quaternion targetRotation =
            Quaternion.Euler(tiltX, 90f, -90f);

        for (int i = 0; i < slats.Length - 1; i++)
        {
            slats[i].localRotation = targetRotation;
        }
    }
}