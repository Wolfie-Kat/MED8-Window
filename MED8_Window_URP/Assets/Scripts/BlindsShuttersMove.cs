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

    void Start()
    {
        int count = slats.Length;

        startY = new float[count];
        slatHeights = new float[count];

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

        openAmount = Mathf.MoveTowards(
            openAmount,
            targetOpen,
            liftSpeed * Time.deltaTime);

        UpdateSlats();
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
}