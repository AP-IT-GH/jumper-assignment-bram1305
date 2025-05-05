using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    [SerializeField] private float endX = 5f;
    [SerializeField] private float minSpeed = 1f; // minimum speed limit
    [SerializeField] private float maxSpeed = 5f; // maximum speed limit
    private Vector3 initialPosition;
    private bool moving = false;
    private float currentSpeed;


    void Start()
    {
        currentSpeed = Random.Range(minSpeed, maxSpeed);
        if (minSpeed > maxSpeed)
        {
            Debug.LogWarning("ObstacleController: minSpeed is greater than maxSpeed. Setting minSpeed = maxSpeed.", this);
            minSpeed = maxSpeed;
        }
        if (currentSpeed <= 0)
        {
            Debug.LogWarning("ObstacleController: Initial speed is zero or negative. Obstacle may not move correctly.", this);
        }

    }

    void Update()
    {
        // Move towards endX position using current instance's speed
        transform.position += Vector3.right * currentSpeed * Time.deltaTime;

        // Check obstacle has passed end position
        if (transform.position.x >= endX)
        {
            // Destroy itself when end reached
            Destroy(gameObject);
        }
    }
}
