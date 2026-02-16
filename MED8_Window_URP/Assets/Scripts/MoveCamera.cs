using System;
using Mono.Cecil.Cil;
using Unity.Collections;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    //----------------------------------------------------------------------------------------------------------//
    // Variables
    [SerializeField] private UnityPythonConnector gameManager;
    [SerializeField] private float verticalAngle;
    [SerializeField] private float horizontalAngle;

    //----------------------------------------------------------------------------------------------------------//
    // Functions

    //Tilting the camera
    void TiltCam(float xCoordinate, float yCoordinate)
    {
        Vector2 normalizedValues = Normalize(xCoordinate, yCoordinate, verticalAngle, horizontalAngle);
        transform.rotation = Quaternion.Euler(normalizedValues.x, normalizedValues.y, 0f);
    }

    //Normalize x coordinates from python to unity
    Vector2 Normalize(float xCoordinate, float yCoordinate, float vertical, float horizontal)
    {

        return new Vector2(
            -vertical + (yCoordinate - 0f) * (vertical - (-vertical)) / (1f - 0f),
            -horizontal + (xCoordinate - 0f) * (horizontal - (-horizontal)) / (1f - 0f)
            );
    }

    //----------------------------------------------------------------------------------------------------------//
    // Start is called before the first frame update
    void Start()
    {
        
    }
    //----------------------------------------------------------------------------------------------------------//
    // Update is called once per frame
    void Update()
    {
        TiltCam(gameManager.receivedValue.x, gameManager.receivedValue.y);

        // Rounding the values to 2 decimals
        // float tempx = (float)Math.Round(gameManager.receivedValue.x, 2);
        // float tempy = (float)Math.Round(gameManager.receivedValue.y, 2);
        // TiltCam(tempx, tempy);
    }
    //----------------------------------------------------------------------------------------------------------//
}
