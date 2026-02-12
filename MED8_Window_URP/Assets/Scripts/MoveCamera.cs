using System;
using Mono.Cecil.Cil;
using Unity.Collections;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    //----------------------------------------------------------------------------------------------------------//
    // Functions

    //Moving the camera
    void MoveCam(float xCoordinate, float yCoordinate)
    {
        transform.position = new Vector3(NormalizeX(xCoordinate), NormalizeY(yCoordinate), transform.position.z);
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
    }
    //----------------------------------------------------------------------------------------------------------//
}
