using System;
using Mono.Cecil.Cil;
using Unity.Collections;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    //----------------------------------------------------------------------------------------------------------//
    // Variables
    [SerializeField] private UnityPythonConnector gameManager;
    [SerializeField] private ProjectionPlaneCamera _projectionPlaneCamera;
    [SerializeField] private GameObject projectionPlane;
    
    [Header("Movement Settings")]
    public Vector3 PlaneOrigin; // Set this in Start() or the Inspector
    public float MovementScale = 1.0f;
    
    [Header("Calibration Settings")]
    public float WebcamHorizontalFOV = 60f; // Typical webcam FOV
    public float ScreenPhysicalWidth = 0.5f; // Physical width of your monitor in meters
    
    [Header("Natural Ratios")]
    [Range(0, 1)] public float WindowMovementWeight = 0.9f; // How much the PLANE moves
    [Range(0, 1)] public float HeadLeaningWeight = 0.1f;    // How much your HEAD moves relative to plane
    public float GlobalZScale = 0.05f;
    
    private Transform _headTransform;

    //----------------------------------------------------------------------------------------------------------//
    
    void Start()
    {
        if (projectionPlane != null)
            PlaneOrigin = projectionPlane.transform.position;
        
        _headTransform = transform;
    }
    
    // Functions

    void MoveCam(float xNorm, float yNorm)
    {
        var screen = _projectionPlaneCamera.ProjectionScreen;
    
        // 1. Get distance in meters (assuming Python sends cm)
        float distMeters = gameManager.distance * 0.01f;

        // Map 0...1 to -HalfFOV... +HalfFOV
        float angleX = (0.5f - xNorm) * WebcamHorizontalFOV; 
        float angleY = (0.5f - yNorm) * (WebcamHorizontalFOV / 1.77f); // Adjust for 16:9 aspect

        // 3. Trigonometric Physical Offset (SOH CAH TOA)
        // Physical Offset = Tan(angle) * Distance
        float physicalX = Mathf.Tan(angleX * Mathf.Deg2Rad) * distMeters;
        float physicalY = Mathf.Tan(angleY * Mathf.Deg2Rad) * distMeters;

        // 4. Update Plane and Camera
        // (Apply your Weights here as discussed previously)
        float rawZ = distMeters;
        float planeZ = PlaneOrigin.z + (rawZ * WindowMovementWeight);
        float headZ = (rawZ * HeadLeaningWeight) + 0.1f;

        projectionPlane.transform.position = new Vector3(PlaneOrigin.x, PlaneOrigin.y, planeZ);

        // Position the camera using the calibrated physical offsets
        _headTransform.position = projectionPlane.transform.position +
                             (screen.DirRight * physicalX) + 
                             (screen.DirUp * physicalY) +
                             (screen.DirNormal * headZ);
        
        if (_projectionPlaneCamera != null)
        {
            _projectionPlaneCamera.HeadPosition = _headTransform;
        }
    }

    
    //----------------------------------------------------------------------------------------------------------//
    // Update is called once per frame
    void Update()
    {
        MoveCam(gameManager.receivedValue.x, gameManager.receivedValue.y);

        // Rounding the values to 2 decimals
        // float tempx = (float)Math.Round(gameManager.receivedValue.x, 2);
        // float tempy = (float)Math.Round(gameManager.receivedValue.y, 2);
        // TiltCam(tempx, tempy);
    }
    //----------------------------------------------------------------------------------------------------------//
}
