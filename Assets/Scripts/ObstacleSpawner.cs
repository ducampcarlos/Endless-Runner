using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Pool Settings")]
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject jumpObstaclePrefab;
    [SerializeField] private GameObject slideObstaclePrefab;
    [SerializeField] private List<Vector3> spawnPositions;
    [SerializeField] private int poolSize = 20;

    private List<GameObject> obstaclePool = new List<GameObject>();
    private float gameStartTime;

    [Header("Difficulty Settings")]
    [SerializeField] private float initialSpawnInterval = 1f;
    [SerializeField] private float minSpawnInterval = 0.3f;
    [SerializeField] private float spawnIntervalDropRate = 0.05f;

    [SerializeField] private float initialEnemySpeed = 5f;
    [SerializeField] private float maxEnemySpeed = 15f;
    [SerializeField] private float enemySpeedUpRate = 0.5f;

    private float spawnInterval;
    private float currentSpeed;
    private bool spawning = false;

    private void Start()
    {
        PreparePool();
    }

    public void StartSpawning()
    {
        gameStartTime = Time.timeSinceLevelLoad;
        spawning = true;
        StartCoroutine(SpawnObstacles());
    }

    public void StopSpawning()
    {
        spawning = false;
        StopAllCoroutines();
    }

    private void PreparePool()
    {
        int normalCount = 12;
        int jumpCount = 4;
        int slideCount = 4;

        for (int i = 0; i < normalCount; i++)
            AddToPool(obstaclePrefab);

        for (int i = 0; i < jumpCount; i++)
            AddToPool(jumpObstaclePrefab);

        for (int i = 0; i < slideCount; i++)
            AddToPool(slideObstaclePrefab);
    }

    private void AddToPool(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);
        obj.SetActive(false);
        obstaclePool.Add(obj);
    }

    IEnumerator SpawnObstacles()
    {
        while (spawning)
        {
            float t = Time.timeSinceLevelLoad - gameStartTime;

            spawnInterval = Mathf.Max(minSpawnInterval, initialSpawnInterval - t * spawnIntervalDropRate);
            currentSpeed = Mathf.Min(maxEnemySpeed, initialEnemySpeed + t * enemySpeedUpRate);

            SpawnFromPool(currentSpeed);

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnFromPool(float speed)
    {
        List<GameObject> inactive = obstaclePool.FindAll(obj => !obj.activeInHierarchy);

        if (inactive.Count == 0) return;

        GameObject toSpawn = inactive[Random.Range(0, inactive.Count)];

        Vector3 spawnPos = spawnPositions[Random.Range(0, spawnPositions.Count)];

        if (toSpawn.CompareTag(slideObstaclePrefab.tag))
        {
            spawnPos.y += 1;
        }
        else if (toSpawn.CompareTag(obstaclePrefab.tag))
        {
            spawnPos.x += Random.Range(-1.8f, 1.8f);
            spawnPos.y += 0.5f;
        }

        toSpawn.transform.position = spawnPos;
        toSpawn.SetActive(true);

        Enemy mover = toSpawn.GetComponent<Enemy>();
        if (mover != null)
            mover.MoveSpeed = speed;
    }

    public float GetCurrentSpeed() => currentSpeed;
}
