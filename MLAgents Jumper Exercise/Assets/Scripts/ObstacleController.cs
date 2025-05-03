using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    [SerializeField] private float startX = -5f;
    [SerializeField] private float endX = 5f;
    [SerializeField] private float speed = 3f; // Adjust speed as needed
    [SerializeField] private JumperAgent targetAgent; // Assign the Agent in the Inspector

    private Vector3 initialPosition;
    private bool moving = false;

    void Awake()
    {
        // Store initial Y and Z based on placement in the scene
        initialPosition = new Vector3(startX, transform.position.y, transform.position.z);
    }

    void Start()
    {
        // Ensure it starts correctly if the scene starts playing immediately
        ResetObstacle();
    }

    void Update()
    {
        if (moving)
        {
            // Move smoothly towards the endX position
            transform.position += Vector3.right * speed * Time.deltaTime;

            // Check if the obstacle has reached or passed the end position
            if (transform.position.x >= endX)
            {
                // Snap to exact end position
                transform.position = new Vector3(endX, transform.position.y, transform.position.z);
                moving = false; // Stop moving

                // Notify the agent of success if it's assigned
                if (targetAgent != null)
                {
                    // Check if the agent is still active (hasn't been punished already)
                    // This check might not be strictly necessary depending on exact timing,
                    // but can prevent rewarding an already ended episode.
                    if (targetAgent.gameObject.activeInHierarchy)
                    {
                        targetAgent.ObstacleReachedEnd();
                    }
                }
                // Optional: Deactivate obstacle until next reset, or just let it sit there.
                // gameObject.SetActive(false);
            }
        }
    }

    // Called by the Agent at the start of each episode
    public void ResetObstacle()
    {
        transform.position = initialPosition;
        moving = true;
        // Ensure it's active if you deactivated it on success
        // gameObject.SetActive(true);
    }

    // Optional: Add a Rigidbody to the obstacle and mark it IsKinematic if you
    // want it to trigger OnCollisionEnter reliably without being affected by physics.
    // Make sure it also has a Collider component.
}
