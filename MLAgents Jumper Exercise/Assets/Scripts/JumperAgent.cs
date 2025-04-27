using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class JumperAgent : Agent
{
    public GameObject Obstacle;
    public GameObject Floor;
    public float SpeedMultiplier = 0.1f;
    public float RotationMultiplier = 0.1f; // This is currently unused but kept for potential future use
    public float JumpForce = 5.0f;
    private Rigidbody rb;
    private bool isGrounded;
    private bool shouldEndEpisode = false; // Flag to defer episode ending

    // Removed 'orientation' variable
    // Removed 'startLower' variable as it was tied to the orientation logic

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        // *** FIX: Set CollisionDetectionMode to Continuous for better physics stability at speed ***
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        else
        {
            Debug.LogError("JumperAgent requires a Rigidbody component.");
        }
    }

    public override void OnEpisodeBegin()
    {
        #region Fallen agent position reset
        if (this.transform.localPosition.y < 0)
        {
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
            this.transform.localRotation = Quaternion.identity;
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero; // Also reset angular velocity
            }
        }
        #endregion

        #region Obstacle position (Fixed Axis)
        // Obstacle will now always be placed on the Z-axis
        Collider agentCollider = this.GetComponent<Collider>();
        Collider obstacleCollider = Obstacle.GetComponent<Collider>();
        bool pickingCoordinate = true;
        float coordinate = 0.0f;

        // Ensure colliders are valid before accessing bounds
        if (agentCollider == null)
        {
            Debug.LogError("Agent requires a Collider component.");
            // Potentially end episode or handle error
            // Do not call EndEpisode here, might cause a loop if called from OnEpisodeBegin somehow
            return;
        }
        if (obstacleCollider == null)
        {
            Debug.LogError("Obstacle requires a Collider component.");
            // Potentially end episode or handle error
            return;
        }


        while (pickingCoordinate) // We don't want a collision between agent and obstacle upon spawning the obstacle, and we want to give our agent sufficient space
        {
            Debug.Log("picking coordinate");
            // Adjusted distance check for Z-axis placement
            // Calculate the minimum required distance along the Z-axis to prevent overlap
            float minDistanceZ = agentCollider.bounds.extents.z + obstacleCollider.bounds.extents.x + 0.5f; // Added a small buffer

            // Let's ensure the obstacle is placed at a positive Z coordinate sufficiently far from the agent's initial Z.
            // We'll place the obstacle between 3 and 5.5 units in front of the agent's start position (assuming agent starts at Z=0).
            float obstacleZPosition = Random.Range(3.0f, 5.5f);


            // Simplified placement: Always place obstacle in front of the agent on the Z axis
            // Ensure the obstacle is far enough away
            if (obstacleZPosition > this.transform.localPosition.z + minDistanceZ)
            {
                coordinate = obstacleZPosition;
                pickingCoordinate = false;
            }
            else
            {
                // If the random position wasn't far enough, try again or adjust
                // For simplicity, let's just pick again. A more robust solution might guarantee a valid placement.
                continue;
            }
        }

        // Obstacle rotation fixed as it's now always on the Z-axis for jumping over
        Obstacle.transform.rotation = Quaternion.Euler(Obstacle.transform.eulerAngles.x, 90f, Obstacle.transform.eulerAngles.z);
        // Obstacle position fixed to the Z-axis
        Obstacle.transform.position = new Vector3(0.0f, 0.5f, coordinate);
        Obstacle.SetActive(true);

        // Ensure the deferral flag is reset at the start of a new episode
        shouldEndEpisode = false;

        // Removed startLower logic
        #endregion
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(this.transform.forward); // Agent's forward direction
        sensor.AddObservation(rb.velocity.y); // Agent's vertical velocity
        // Added obstacle position observation as the orientation is no longer random
        sensor.AddObservation(Obstacle.transform.localPosition);
        sensor.AddObservation(isGrounded); // Add isGrounded observation
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector3 controlSignal = Vector3.zero;
        // Continuous action 0: controls forward/backward movement along the agent's Z axis
        controlSignal.z = actionBuffers.ContinuousActions[0];
        // Continuous action 1: controls sideways movement along the agent's X axis
        controlSignal.x = actionBuffers.ContinuousActions[1];

        // Use Rigidbody.MovePosition for physics-based movement, which is generally better
        // when combining with AddForce and dealing with collisions, especially with Continuous collision detection.
        // Calculate the target position based on current position and control signal
        Vector3 moveDirection = transform.TransformDirection(controlSignal);
        Vector3 targetPosition = rb.position + moveDirection * SpeedMultiplier * Time.fixedDeltaTime; // Use Time.fixedDeltaTime with physics updates

        // Only move if the episode is not pending end, to avoid moving into a bad state just before reset
        if (!shouldEndEpisode)
        {
            rb.MovePosition(targetPosition);
        }


        // Removed the rotation based on continuous action 1 as it conflicted with translation
        // If you still want rotation, you'll need a separate continuous action or modify the movement logic.

        // Discrete action 0: controls jumping (1 for jump, 0 for no jump)
        // Only allow jump if grounded and not pending episode end
        if (actionBuffers.DiscreteActions[0] == 1 && isGrounded && !shouldEndEpisode)
        {
            // Apply jump force using ForceMode.Impulse for an instant force change
            rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
            isGrounded = false; // Immediately set isGrounded to false after jumping
        }

        // Reward condition: check if the agent has passed the obstacle on the Z-axis
        // This assumes the agent starts near Z=0 and the obstacle is placed at a positive Z.
        // Check if the agent's Z position is greater than the obstacle's Z position.
        // Only give positive reward if not pending episode end (e.g., already hit obstacle)
        if (!shouldEndEpisode && this.transform.localPosition.z > Obstacle.transform.localPosition.z + Obstacle.GetComponent<Collider>().bounds.extents.x / 2f) // Add half obstacle size for clearer passing condition
        {
            SetReward(1.0f);
            shouldEndEpisode = true; // Defer ending episode after successful pass
        }

        // Optional: Add a small negative reward for time to encourage faster completion
        // Only add time penalty if not pending episode end
        if (!shouldEndEpisode)
        {
            AddReward(-0.001f); // Small negative reward per step
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        var discreteActionsOut = actionsOut.DiscreteActions;

        // Map keyboard input to continuous actions for heuristic mode
        continuousActionsOut[0] = Input.GetAxis("Vertical"); // Forward/Backward
        continuousActionsOut[1] = Input.GetAxis("Horizontal"); // Left/Right

        // Map Space key to discrete jump action
        discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    // Use FixedUpdate for physics-related checks and deferred actions
    void FixedUpdate()
    {
        // *** FIX: Check the flag here and end the episode outside of the immediate collision callback ***
        if (shouldEndEpisode)
        {
            shouldEndEpisode = false; // Reset the flag
            EndEpisode();             // End the episode in FixedUpdate
        }

        // Additional check for isGrounded based on a small raycast or checking distance to the floor
        // This can be more reliable than just OnCollisionEnter/Exit for determining if the agent is truly on the ground.
        // For simplicity, we'll rely on the OnCollisionEnter/Exit/Stay for now, but keep this in mind if ground detection is flaky.
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == Floor)
        {
            isGrounded = true;
        }
        // Optional: Add a negative reward for hitting the obstacle
        if (collision.gameObject == Obstacle)
        {
            SetReward(-1.0f);
            shouldEndEpisode = true; // *** FIX: Set flag instead of calling EndEpisode directly ***
                                     // Do NOT call EndEpisode() here!
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == Floor)
        {
            // Only set isGrounded to false if we are leaving the floor, not just any collision
            // This helps prevent issues if the agent briefly touches another object while not on the floor.
            // A more robust check might be needed for complex environments.
            isGrounded = false;
        }
    }

    // Add OnCollisionStay to ensure isGrounded remains true while touching the floor
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject == Floor)
        {
            isGrounded = true;
        }
    }
}