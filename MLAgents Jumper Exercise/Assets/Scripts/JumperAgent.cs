using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class JumperAgent : Agent
{
    [Header("Movement")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private Transform groundCheck; // Assign empty GameObject slightly below agent
    [SerializeField] private LayerMask groundLayer; // Set this to layer ground plane is on
    [SerializeField] private float groundCheckRadius = 0.1f;


    [Header("Rewards")]
    [SerializeField] private float touchPunishment = -1.0f;
    [SerializeField] private float survivalReward = 0.001f;

    private Rigidbody rb;
    private Vector3 initialPosition;
    private bool isGrounded;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        initialPosition = transform.localPosition; // Store starting position relative to parent (if any)

        if (groundCheck == null)
        {
            Debug.LogError("GroundCheck transform is not assigned!", this);
        }
    }

    public override void OnEpisodeBegin()
    {
        // Reset Agent
        transform.localPosition = initialPosition;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Cleanup obstacles from previous episode
        ObstacleController[] obstaclesInChildren = GetComponentsInChildren<ObstacleController>();
        foreach (ObstacleController obstacleController in obstaclesInChildren)
        {
            // Make sure we don't destroy the prefab itself if it's somehow a child
            if (obstacleController.gameObject != null && obstacleController.gameObject.CompareTag("Obstacle"))
            {
                Destroy(obstacleController.gameObject);
            }
        }
    }

    // Observation Collection (Using Ray Perception Sensor Component)
    // We can add extra vector observations here if needed.
    public override void CollectObservations(VectorSensor sensor)
    {
        // Example Optional Observations (Uncomment if needed):
        sensor.AddObservation(isGrounded); // Agent knows if it can jump (1 bool = 1 obs value)
        sensor.AddObservation(rb.velocity.y /10f); // Agent knows its vertical speed
        // sensor.AddObservation(transform.localPosition.y); // Agent knows its height

        // Ray Perception Sensor component will add its observations automatically.
    }

    // Action Handling
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Check if grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        // Action: 0 = Do Nothing, 1 = Jump
        int jumpAction = actions.DiscreteActions[0];
        Debug.Log(jumpAction);
        Debug.Log("ActionReceived");
        if (jumpAction == 1)
        {
            Debug.Log("Jump action activated");
            if (isGrounded is true)
            {
                Debug.Log(isGrounded + "isGrounded");
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }

        }
        AddReward(survivalReward); // Add reward for surviving step
        // Apply small penalty for existing to encourage efficiency
        // AddReward(stepPenalty);
    }

    // Manual Control (for testing)
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0; // Jump with Spacebar
    }

    // Collision Detection
    void OnCollisionEnter(Collision collision)
    {
        // Check if collision is with obstacle
        if (collision.gameObject.CompareTag("Obstacle")) // Make sure Obstacle has "Obstacle" tag
        {
            Debug.Log("Agent touched Obstacle - Punishing and Ending Episode.");
            AddReward(touchPunishment);
            EndEpisode(); // End episode immediately on failure
        }
    }

    // Gizmo: visualize ground check sphere in editor
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}