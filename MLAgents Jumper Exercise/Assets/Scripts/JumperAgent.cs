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
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.1f;

    [Header("Rewards")]
    [SerializeField] private float touchPunishment = -0.01f;
    [SerializeField] private float obstacleClearedReward = 0.01f;

    [Header("Environment Control")]
    [SerializeField] private ObstacleSpawner obstacleSpawner;
    [SerializeField] private int MaxCollisions = 3;
    private int CurrentCollisions = 0;

    private Rigidbody rb;
    private Vector3 initialPosition;
    private bool isGrounded;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        initialPosition = transform.localPosition;

        if (groundCheck == null) Debug.LogError("GroundCheck transform is not assigned!", this);
        if (obstacleSpawner == null) Debug.LogError("ObstacleSpawner is not assigned to the JumperAgent!", this);
    }

    public override void OnEpisodeBegin()
    {
        CurrentCollisions = 0;
        Debug.Log("Episode Begin");
        transform.localPosition = initialPosition;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Make spawner clear old obstacles and reset
        if (obstacleSpawner != null)
        {
            obstacleSpawner.ClearObstacles(); // Spawner's method
            obstacleSpawner.ResetSpawner();
        }
        else // Old logic in case spawner doesn't work.
        {
            ObstacleController[] allObstacles = FindObjectsOfType<ObstacleController>();
            foreach (ObstacleController obstacleController in allObstacles)
            {
                if (obstacleController.gameObject != null && obstacleController.gameObject.CompareTag("Obstacle"))
                {
                    Destroy(obstacleController.gameObject);
                }
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(isGrounded);
        sensor.AddObservation(rb.velocity.y / 10f);
    }

    public override void OnActionReceived(ActionBuffers actions) // Jumping logic
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        int jumpAction = actions.DiscreteActions[0];
        if (jumpAction == 1 && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    // Called by obstacle controller
    public void RewardForObstacleCleared()
    {
        AddReward(obstacleClearedReward);
        // Debug.Log("Agent rewarded for obstacle cleared!");
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Collision entered.");
            CurrentCollisions += 1;
            AddReward(touchPunishment);
            // No episode end, if we end episode here, agent might never learn: evading obstacle will result in better cumulative reward
        }
        if (CurrentCollisions == MaxCollisions)
        {
            EndEpisode();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}