using UnityEngine;

public class BlindsShutterMove : MonoBehaviour
{
    [Header("Blinds")]
    public Transform[] slats;   // assign top -> bottom
    public float moveSpeed = 1f;

    [Header("Collapse Settings")]
    public float collapsedSpacing = 0.02f; // spacing when stacked

    [Range(0f, 1f)]
    public float openAmount = 0f; // 0 = down, 1 = collapsed

    float targetOpen;

    Vector3[] startPositions;

    void Start()
    {
        startPositions = new Vector3[slats.Length];

        for (int i = 0; i < slats.Length; i++)
            startPositions[i] = slats[i].localPosition;
    }

    void Update()
    {
        // TEST INPUT
        if (Input.GetKey(KeyCode.W))
            targetOpen += moveSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.S))
            targetOpen -= moveSpeed * Time.deltaTime;

        targetOpen = Mathf.Clamp01(targetOpen);

        // Smooth movement
        openAmount = Mathf.MoveTowards(openAmount, targetOpen, moveSpeed * Time.deltaTime);

        UpdateSlats();
    }

    void UpdateSlats()
    {
        int count = slats.Length;

        // where the stack begins (top slat reference)
        float currentStackY = startPositions[0].y;

        for (int i = 0; i < count; i++)
        {
            float normalizedIndex = (float)i / (count - 1);

            // weight controls how fast each slat joins stack
            float minWeight = 0.2f;
            float weight = Mathf.Lerp(minWeight, 1f, normalizedIndex);

            // how much this slat wants to rise
            float lift = openAmount * weight;

            Vector3 pos = startPositions[i];

            // target lifted position (free movement)
            float liftedY = Mathf.Lerp(
                startPositions[i].y,
                startPositions[0].y,
                lift
            );

            // TRUE STACKING:
            // slat cannot go higher than stack position
            pos.y = Mathf.Min(liftedY, currentStackY);

            slats[i].localPosition = pos;

            // move stack downward by this slat's thickness
            currentStackY -= collapsedSpacing;
        }
    }
}