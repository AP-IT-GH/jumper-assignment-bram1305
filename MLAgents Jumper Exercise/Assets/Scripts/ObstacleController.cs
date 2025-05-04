using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    [SerializeField] private float startX = -5f;
    [SerializeField] private float endX = 5f;
    [SerializeField] private float minSpeed = 1f; // minimum speed limit
    [SerializeField] private float maxSpeed = 5f; // maximum speed limit
    [SerializeField] private JumperAgent targetAgent; // Assign Agent in Inspector

    private Vector3 initialPosition;
    private bool moving = false;
    private float currentSpeed;

    void Awake()
    {
        // Store initial Y and Z based on placement in scene
        initialPosition = new Vector3(startX, transform.position.y, transform.position.z);
    }

    void Start()
    {
        // Ensure it starts correctly if scene starts playing immediately
        ResetObstacle();
    }

    void Update()
    {
        if (moving)
        {
            // Move smoothly towards endX position
            transform.position += Vector3.right * currentSpeed * Time.deltaTime;

            // Check if obstacle reached end position
            if (transform.position.x >= endX)
            {
                // Snap to exact end position
                transform.position = new Vector3(endX, transform.position.y, transform.position.z);
                moving = false; // Stop moving

                // Notify agent of success if it's assigned
                if (targetAgent != null)
                {
                    // Check if agent is still active (hasn't been punished already)
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

    // Called by Agent at start of each episode
    public void ResetObstacle()
    {
        currentSpeed = Random.Range(minSpeed, maxSpeed);
        transform.position = initialPosition;
        moving = true;
    }

}
