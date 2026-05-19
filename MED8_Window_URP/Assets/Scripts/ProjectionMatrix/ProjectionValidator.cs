using UnityEngine;

/// <summary>
/// Quantitative off-axis projection test.
///
/// Places markers at known depths behind the projection plane.
/// Every frame, computes where each marker SHOULD appear on screen
/// (using the math formula) and where it ACTUALLY appears (using the
/// camera). If the difference is less than 1 pixel, the projection
/// is correct.
///
/// The formula:
///   When the eye is offset by dx from the projection plane center,
///   an object at depth z behind the plane appears shifted on the
///   physical projection plane by:
///
///     shift = dx * z / (z + eyeDistance)
///
///   - On the projection plane (z=0): shift = 0 (zero parallax)
///   - At z = eyeDistance: shift = dx/2
///   - At z = infinity: shift = dx (maximum parallax)
///
/// Press V to run the validation at the current eye position.
/// </summary>
public class ProjectionValidator : MonoBehaviour
{
    [Header("References")]
    public ProjectionPlane projectionPlane;
    public Transform eyeTransform;
    public Camera offAxisCamera;

    [Header("Test Points (depths behind projection plane in meters)")]
    public float[] testDepths = { 0f, 0.3f, 0.6f, 1.2f, 2.4f };

    private string _report = "Press V to validate";
    private bool _passed = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
            Validate();
    }

    void Validate()
    {
        if (projectionPlane == null || eyeTransform == null || offAxisCamera == null)
        {
            _report = "Missing references";
            return;
        }

        Vector3 eyePos = eyeTransform.position;
        Vector3 planeCenter = (projectionPlane.BottomLeft + projectionPlane.TopRight) * 0.5f;
        Vector3 eyeToCenter = eyePos - planeCenter;

        // Eye offset along projection plane axes
        float dx = Vector3.Dot(eyeToCenter, projectionPlane.DirRight);
        float dy = Vector3.Dot(eyeToCenter, projectionPlane.DirUp);
        float eyeDist = -Vector3.Dot(projectionPlane.BottomLeft - eyePos, projectionPlane.DirNormal);

        _report = $"Eye offset: dx={dx:F4}m  dy={dy:F4}m  dist={eyeDist:F4}m\n";
        _report += $"Screen: {Screen.width}x{Screen.height}\n\n";
        _report += $"{"Depth",6} | {"Theory X",10} {"Theory Y",10} | {"Actual X",10} {"Actual Y",10} | {"Err px",8} | Result\n";
        _report += new string('-', 85) + "\n";

        _passed = true;

        foreach (float depth in testDepths)
        {
            // World position of test point: center of projection plane + depth behind it
            Vector3 worldPos = planeCenter - projectionPlane.DirNormal * depth;

            // --- Theoretical screen position ---
            // The test point projects onto the projection plane at:
            //   projectedX = dx * depth / (depth + eyeDist)  (shift from center)
            //   projectedY = dy * depth / (depth + eyeDist)
            // This is in meters on the physical projection plane.
            float theoreticalShiftX = dx * depth / (depth + eyeDist);
            float theoreticalShiftY = dy * depth / (depth + eyeDist);

            // Convert from meters-on-plane to pixel position.
            // The projection plane spans the full viewport, so:
            //   pixel = (0.5 + shift / planeSize) * screenResolution
            float theoryPixelX = (0.5f + theoreticalShiftX / projectionPlane.SizeInMeters.x) * Screen.width;
            float theoryPixelY = (0.5f + theoreticalShiftY / projectionPlane.SizeInMeters.y) * Screen.height;

            // --- Actual screen position from camera ---
            Vector3 screenPos = offAxisCamera.WorldToScreenPoint(worldPos);
            float actualPixelX = screenPos.x;
            float actualPixelY = screenPos.y;

            // --- Compare ---
            float errX = Mathf.Abs(theoryPixelX - actualPixelX);
            float errY = Mathf.Abs(theoryPixelY - actualPixelY);
            float errTotal = Mathf.Sqrt(errX * errX + errY * errY);

            bool ok = errTotal < 1f;
            if (!ok) _passed = false;

            _report += $"{depth,6:F2} | {theoryPixelX,10:F2} {theoryPixelY,10:F2} | " +
                       $"{actualPixelX,10:F2} {actualPixelY,10:F2} | {errTotal,8:F3} | " +
                       $"{(ok ? "PASS" : "FAIL")}\n";
        }

        _report += $"\n{(_passed ? "ALL PASS" : "SOME FAILED")}";
        Debug.Log(_report);
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = _passed ? Color.green : Color.red;
        style.richText = false;

        // Background box
        GUI.Box(new Rect(5, 60, 700, 30 + testDepths.Length * 20 + 60), "");
        GUI.Label(new Rect(10, 65, 690, 400), _report, style);
    }
}
