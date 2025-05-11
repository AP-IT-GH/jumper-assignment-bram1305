using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    [SerializeField] private float endX = 5f;
    [SerializeField] private float minSpeed = 1f;
    [SerializeField] private float maxSpeed = 5f;
    private float currentSpeed;

    // Callback invoked when obstacle reaches end
    public System.Action OnObstacleReachedEnd;

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
        transform.position += Vector3.right * currentSpeed * Time.deltaTime;

        if (transform.position.x >= endX)
        {
            // Invoke callback before destroying the object
            OnObstacleReachedEnd?.Invoke();
            Destroy(gameObject);
        }
    }
}
