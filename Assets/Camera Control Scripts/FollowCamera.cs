using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] Transform t;

    /*private void Update()
    {
        //t.position = cam.transform.position;

        // Get the camera's position
        Vector3 newPosition = cam.transform.position;

        // Keep the sprite's original z position (usually z = 0 for 2D games)
        newPosition.z = t.position.z;

        // Update the sprite's position to follow the camera
        t.position = newPosition;
    }*/
}
