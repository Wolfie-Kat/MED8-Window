using System;
using Mono.Cecil.Cil;
using Unity.Collections;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    //----------------------------------------------------------------------------------------------------------//
    // Variables
    [SerializeField] private UnityPythonConnector gameManager;
    [SerializeField] private float horizontalRange = 2f; // movement left/right
    [SerializeField] private float verticalRange = 2f;   // movement up/down
    [SerializeField] private float depth = 0f;           // optional forward/back

    private Vector3 startPosition = new Vector3(0, 0, -5);

    //----------------------------------------------------------------------------------------------------------//
    
    void Start()
    {
        
    }
    
    // Functions

    void MoveCam(float x, float y)
    {
        float posX = Mathf.Lerp(horizontalRange, -horizontalRange, x);
        float posY = Mathf.Lerp(verticalRange, -verticalRange, y);

        transform.localPosition = startPosition + new Vector3(posX, posY, 0f);
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
