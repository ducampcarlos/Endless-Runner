using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject enemyPrefab; // Prefab for the enemy
    [SerializeField] private List<Vector3> enemySpawns; // List of spawn points for the enemies
    [SerializeField] private float spawnInterval = 1f; // Time interval between enemy spawns

    private void Awake()
    {
        // Ensure that there is only one instance of GameManager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        StartCoroutine(SpawnEnemies()); // Start spawning enemies
    }


    public void Spawn()
    {
        Instantiate(enemyPrefab, enemySpawns[Random.Range(0, enemySpawns.Count)], Quaternion.identity);
    }

    IEnumerator SpawnEnemies()
    {
        while (true)
        {
            Spawn();
            yield return new WaitForSeconds(spawnInterval); // Wait for 1 second before spawning the next enemy
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Restart the current scene
    }
}
