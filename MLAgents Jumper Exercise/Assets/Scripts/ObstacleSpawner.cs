using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject obstaclePrefab; // Assign Obstacle prefab
    [SerializeField] private float spawnX = -5f; // X position where obstacles appear
    [SerializeField] private Transform spawnPoint; // Assign empty GameObject for Y/Z position reference

    [Header("Spawn Timing")]
    [SerializeField] private float initialSpawnDelay = 1.0f;
    [SerializeField] private float minSpawnInterval = 1.5f;
    [SerializeField] private float maxSpawnInterval = 3.0f;

    private float timeSinceLastSpawn;
    private float currentSpawnInterval;
    private bool firstSpawnDone = false;

    void Start()
    {
        if (obstaclePrefab == null)
        {
            Debug.LogError("Obstacle Prefab is not assigned!", this);
            enabled = false; // Disable spawner if prefab is missing
            return;
        }
        if (spawnPoint == null)
        {
            Debug.LogError("Spawn Point transform is not assigned!", this);
            enabled = false; // Disable spawner if spawn point is missing
            return;
        }

        // Validate intervals
        if (minSpawnInterval > maxSpawnInterval)
        {
            Debug.LogWarning("minSpawnInterval is greater than maxSpawnInterval. Using maxSpawnInterval as min.", this);
            minSpawnInterval = maxSpawnInterval;
        }
        if (minSpawnInterval < 0) minSpawnInterval = 0;


        // Initialize timer based on initial delay
        timeSinceLastSpawn = -initialSpawnDelay; // Start negative to account for initial delay
        SetNextSpawnInterval();
    }

    void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;

        // Initial delay logic is single-use
        if (!firstSpawnDone)
        {
            if (timeSinceLastSpawn >= 0) // Initial delay passed
            {
                SpawnObstacle();
                firstSpawnDone = true;
                // Reset timer *after* first spawn
                timeSinceLastSpawn = 0f;
                SetNextSpawnInterval();
            }
        }
        // Regular spawning after first spawn
        else if (timeSinceLastSpawn >= currentSpawnInterval)
        {
            SpawnObstacle();
            timeSinceLastSpawn = 0f; // Reset timer
            SetNextSpawnInterval(); // Pick time for *next* spawn
        }
    }

    void SetNextSpawnInterval()
    {
        currentSpawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    void SpawnObstacle()
    {
        if (obstaclePrefab == null || spawnPoint == null) return; // Safety check

        Vector3 spawnPosition = new Vector3(
            spawnPoint.position.x,
            spawnPoint.position.y,
            spawnPoint.position.z
        );

        Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity, transform); // Spawn as child (optional)
    }

    public void ResetSpawner()
    {
        timeSinceLastSpawn = -initialSpawnDelay;
        firstSpawnDone = false;
        SetNextSpawnInterval();
        // Does not destroy existing spawned obstacles
    }
}
