using System;
using Mono.Cecil.Cil;
using Unity.Collections;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] private UnityPythonConnector gameManager;
    //----------------------------------------------------------------------------------------------------------//
    // Functions

    //Moving the camera
    void MoveCam(float xCoordinate, float yCoordinate)
    {
        transform.position = new Vector3(NormalizeX(xCoordinate), NormalizeY(yCoordinate), transform.position.z);
    }

    //Tilting the camera
    void TiltCam(float xCoordinate, float yCoordinate)
    {
        float tiltNormalizedX = 30f + (yCoordinate - 0f) * ((-30f) - 30f) / (1f - 0f);
        float tiltNormalizedY = -30f + (xCoordinate - 0f) * (30f - (-30f)) / (1f - 0f);
        transform.rotation = Quaternion.Euler(tiltNormalizedX, tiltNormalizedY, 0f);
    }

    //Normalize x coordinates from python to unity
    float NormalizeX(float xCoordinate)
    {
        return -4f + (xCoordinate - 0f) * (4f - (-4f)) / (1f - 0f);
    }

    //Normalize y coordinates from python to unity
    float NormalizeY(float yCoordinate)
    {
        return 0.5f + (yCoordinate - 0f) * (3.5f - 0.5f) / (1f - 0f);
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float randomFloat = UnityEngine.Random.Range(0f, 1f);
            MoveCam(randomFloat, randomFloat);
        }
        //MoveCam(gameManager.receivedValue.x, gameManager.receivedValue.y);
        TiltCam(gameManager.receivedValue.x, gameManager.receivedValue.y);
    }
    //----------------------------------------------------------------------------------------------------------//
}
