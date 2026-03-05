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

    //----------------------------------------------------------------------------------------------------------//
    
    void Start()
    {
        
    }
    
    // Functions

    void MoveCam(float xNorm, float yNorm)
    {
        float width = _projectionPlaneCamera.ProjectionScreen.Size.x;
        float height = _projectionPlaneCamera.ProjectionScreen.Size.y;

        float x = (0.5f - xNorm) * width;
        float y = (0.5f - yNorm) * height;
        float z = (gameManager.distance * 0.01f);

        Vector3 eye =
            _projectionPlaneCamera.ProjectionScreen.transform.position +
            _projectionPlaneCamera.ProjectionScreen.DirRight * x +
            _projectionPlaneCamera.ProjectionScreen.DirUp * y +
            _projectionPlaneCamera.ProjectionScreen.DirNormal * z;

        transform.position = eye;
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
