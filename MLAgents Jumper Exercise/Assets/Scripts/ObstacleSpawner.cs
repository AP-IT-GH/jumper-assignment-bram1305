using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Spawn Timing")]
    [SerializeField] private float initialSpawnDelay = 1.0f;
    [SerializeField] private float minSpawnInterval = 1.5f;
    [SerializeField] private float maxSpawnInterval = 3.0f;

    [Header("Agent Reference")]
    [SerializeField] private JumperAgent agentToNotify; // Assign your JumperAgent here in the Inspector

    private float timeSinceLastSpawn;
    private float currentSpawnInterval;
    private bool firstSpawnDone = false;

    void Start()
    {

        if (minSpawnInterval > maxSpawnInterval)
        {
            minSpawnInterval = maxSpawnInterval;
        }
        if (minSpawnInterval < 0) minSpawnInterval = 0;

        timeSinceLastSpawn = -initialSpawnDelay;
        SetNextSpawnInterval();
    }

    void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;

        if (!firstSpawnDone)
        {
            if (timeSinceLastSpawn >= 0)
            {
                SpawnObstacle();
                firstSpawnDone = true;
                timeSinceLastSpawn = 0f;
                SetNextSpawnInterval();
            }
        }
        else if (timeSinceLastSpawn >= currentSpawnInterval)
        {
            SpawnObstacle();
            timeSinceLastSpawn = 0f;
            SetNextSpawnInterval();
        }
    }

    void SetNextSpawnInterval()
    {
        currentSpawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    void SpawnObstacle()
    {
        if (obstaclePrefab == null || spawnPoint == null) return;

        Vector3 spawnPosition = new Vector3(
            spawnPoint.position.x,
            spawnPoint.position.y,
            spawnPoint.position.z
        );

        GameObject newObstacleGO = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity, transform);
        ObstacleController obstacleController = newObstacleGO.GetComponent<ObstacleController>();

        if (obstacleController != null)
        {
            // If agent is assigned, subscribe its reward method to obstacle's event
            if (agentToNotify != null)
            {
                obstacleController.OnObstacleReachedEnd += agentToNotify.RewardForObstacleCleared;
            }
        }
        else
        {
            Debug.LogError("Spawned obstacle prefab does not have an ObstacleController component!", newObstacleGO);
        }
    }

    public void ResetSpawner()
    {
        timeSinceLastSpawn = -initialSpawnDelay;
        firstSpawnDone = false;
        SetNextSpawnInterval();
    }

    // Clear obstacles
    public void ClearObstacles()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Obstacle")) // Make sure your obstacle prefab has Obstacle tag
            {
                Destroy(child.gameObject);
            }
        }
    }
}
