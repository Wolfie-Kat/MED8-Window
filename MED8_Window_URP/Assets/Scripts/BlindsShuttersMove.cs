using System;
using UnityEngine;

/// <summary>Controls lifting and tilting behaviour of window blind slats.</summary>
public class BlindsShutterMove : MonoBehaviour
{
    // Slats ordered TOP to BOTTOM
    [Header("Assign TOP to BOTTOM")]
    public Transform[] slats;

    // ---------- Movement ----------
    [Header("Movement")]

    // 0 = closed, 1 = lifted
    [Range(0f, 1f)]
    public float openAmount = 0f;

    // Smoothed target value
    float targetOpen;

    // Original Y positions of slats
    float[] startY;

    // Slat thickness
    float[] slatHeights;

    // ---------- Tilt ----------
    [Header("Tilt")]

    // Tilt rotation speed
    public float tiltSpeed = 1f;

    // 0–1 tilt control
    [Range(0f,1f)]
    public float tiltAmount = 0.5f;
    // Tilt angle limits
    public float minTiltX = -175f;
    public float maxTiltX = -5f;
    // Smoothed tilt target
    float targetTilt;

    // ---------- External Input ----------
    [Header("External Input")]
    [SerializeField] private UnityPythonConnector _pythonConnector;
    bool gestureActive = false;
    float gestureStartOpen;   // blinds position when gesture begins

    // ----------------------- Initialization -------------------------//
    void Start()
    {
        int count = slats.Length;

        startY = new float[count];
        slatHeights = new float[count];
        targetTilt = tiltAmount;

        // Store starting positions and detect thickness
        for (int i = 0; i < count; i++)
        {
            startY[i] = slats[i].localPosition.y;

            Renderer r = slats[i].GetComponentInChildren<Renderer>();

            // Use renderer bounds if available
            if (r != null)
                slatHeights[i] = r.bounds.size.y;
            else
                slatHeights[i] = 0.02f; // fallback thickness
        }
    }

    // ----------------------- Update Loop -------------------------//
    void Update()
    {
        // Lifting
        bool validGesture =
            _pythonConnector.GesturePosition > -0.5f &&
            _pythonConnector.GestureStartPosition > -0.5f &&
            _pythonConnector.gesture >= 0.0f;

        // Gesture just started
        if (validGesture && !gestureActive)
        {
            gestureActive = true;
            gestureStartOpen = openAmount; // remember blinds position
        }

        // Gesture ended
        if (!validGesture)
        {
            gestureActive = false;
        }

        // Apply relative movement when gesture is active
        if (gestureActive)
        {
            float delta =
                _pythonConnector.GestureStartPosition -
                _pythonConnector.GesturePosition;

            targetOpen = Mathf.Clamp01(gestureStartOpen + delta);
        }

        openAmount = Mathf.MoveTowards(
            openAmount,
            targetOpen,
            Time.deltaTime
        );

        // Tilting
        tiltAmount = Mathf.MoveTowards(
            tiltAmount,
            targetTilt,
            tiltSpeed * Time.deltaTime
        );

        // Apply updates
        UpdateMovement();
        UpdateTilt();
        print(targetOpen);
    }

    //----------------------- Movement & Stacking -------------------------//

    /// <summary>Moves slats upward while enforcing stacking constraints.
    /// Prevents slats from intersecting.</summary>
    void UpdateMovement()
    {
        int count = slats.Length;

        // Start stacking from top slat height
        float stackY = startY[0];

        for (int i = 0; i < count; i++)
        {
            // Normalized index top to bottom
            float normalized = (float)i / (count - 1);

            // Movement weighting
            float weight = Mathf.Lerp(0.9f, 0.9f, normalized);

            // Desired lift position
            float desiredY = Mathf.Lerp(
                startY[i],
                startY[0],
                openAmount * weight
            );

            // Enforce stack constraint
            float finalY = Mathf.Min(desiredY, stackY);

            Vector3 pos = slats[i].localPosition;
            pos.y = finalY;
            slats[i].localPosition = pos;

            // Next slat must sit below this one
            stackY = finalY - slatHeights[i];
        }
    }

    /// <summary>Tilts slats and reduces tilt as blinds are lifted.</summary>
    void UpdateTilt()
    {
        // Tilt influence decreases when opening
        float tiltStrength = 1f - openAmount;

        // Smooth fade-out
        tiltStrength = Mathf.SmoothStep(0f, 1f, tiltStrength);

        // Base tilt from input
        float baseTiltX = Mathf.Lerp(minTiltX, maxTiltX, tiltAmount);

        // Neutral midpoint
        float neutralTilt = (minTiltX + maxTiltX) * 0.5f;

        // Reduce tilt toward neutral
        float tiltX = Mathf.Lerp(neutralTilt, baseTiltX, tiltStrength);

        // Keep Y and Z fixed for slat rotation
        Quaternion targetRotation = Quaternion.Euler(tiltX, 90f, -90f);

        // Apply to all slats except bottom rod
        for (int i = 0; i < slats.Length - 1; i++)
        {
            slats[i].localRotation = targetRotation;
        }
    }

    // Receives lift value from external source (Python). Expected range: 0–1
    public void SetOpenAmount(float value)
    {
        targetOpen = Mathf.Clamp01(value);
    }

    // Receives tilt value from external source (Python). Expected range: 0–1
    public void SetTiltAmount(float value)
    {
        targetTilt = Mathf.Clamp01(value);
    }
    
    // Convenience function for updating both values.
    public void SetBlinds(float open, float tilt)
    {
        targetOpen = Mathf.Clamp01(open);
        targetTilt = Mathf.Clamp01(tilt);
    }
}



//-------------------------------------------------------------------//
//----------------TEMPORARY OLD CODE STORAGE-------------------------//
//-------------------------------------------------------------------//


// void Update()
//     {
//         // ----- Lift input (testing) -----
//         if (Input.GetKey(KeyCode.W))
//             targetOpen += liftSpeed * Time.deltaTime;

//         if (Input.GetKey(KeyCode.S))
//             targetOpen -= liftSpeed * Time.deltaTime;

//         targetOpen = Mathf.Clamp01(targetOpen);

//         // Smooth toward target
//         openAmount = Mathf.MoveTowards(
//             openAmount,
//             targetOpen,
//             liftSpeed * Time.deltaTime
//         );

//         // ----- Tilt input (testing) -----
//         if (Input.GetKey(KeyCode.D))
//             targetTilt += tiltSpeed * Time.deltaTime;

//         if (Input.GetKey(KeyCode.A))
//             targetTilt -= tiltSpeed * Time.deltaTime;

//         targetTilt = Mathf.Clamp01(targetTilt);

//         tiltAmount = Mathf.MoveTowards(
//             tiltAmount,
//             targetTilt,
//             tiltSpeed * Time.deltaTime
//         );

//         // Apply updates
//         UpdateMovement();
//         UpdateTilt();
//     }