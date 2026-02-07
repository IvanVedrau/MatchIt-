using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeRotation : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float rotationSpeed = 10f;
    public Vector3 target;
    private bool isPaused = false;
    private Vector3 currentVelocity = Vector3.zero;
    private const float SMOOTH_TIME = 0.3f;

    // Update is called once per frame
    void Update()
    {
        if (isPaused) return;

        //  for smoother movement
        transform.position = Vector3.SmoothDamp(transform.position, target, ref currentVelocity, SMOOTH_TIME, moveSpeed);
        
        // Update target position
        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            if (target.y != 0.5f)
            {
                target.y = 0.5f;
            }
            else if (target.y == 0.5f)
            {
                target.y = 1.2f;
            }
        }

        // Rotate the cube
        transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
        if (paused)
        {
            // Reset velocity when paused
            currentVelocity = Vector3.zero;
        }
    }
}
