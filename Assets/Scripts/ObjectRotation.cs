using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotation : MonoBehaviour
{
    public bool isPickedUp;
    public float rotationSpeed = 15f; // Speed of rotation in degrees per second

    // Update is called once per frame
    void Update()
    {
        if (!isPickedUp)
        {
            // Rotate smoothly around the Y-axis
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
        }
    }
}