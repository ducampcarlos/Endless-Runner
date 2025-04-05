using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Enemy Spawning")]
    [SerializeField] private GameObject enemyPrefab; // Prefab for the enemy
    [SerializeField] private List<Vector3> enemySpawns; // List of spawn points for the enemies
    [SerializeField] private float spawnInterval = 1f; // Time interval between enemy spawns

    [Header("Score")]
    int score;
    [SerializeField] TextMeshProUGUI scoreText; // Reference to the UI text element for displaying the score

    bool gameStarted = false;

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
        scoreText.gameObject.SetActive(false); // Hide the score text at the start
    }

    private void Update()
    {
        if (!gameStarted)
        {
            //Using the new input system, when any key is pressed, or the screen is touched, call StartGame
#if UNITY_ANDROID || UNITY_EDITOR
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                StartGame();
            }
#endif

#if UNITY_STANDALONE || UNITY_EDITOR
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                StartGame();
            }
#endif
        }
    }

    public void StartGame()
    {
        gameStarted = true;
        scoreText.gameObject.SetActive(true); // Show the score text when the game starts
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

    public void ScoreUp()
    {
        score++;
        scoreText.text = score.ToString(); // Update the score text
    }
}
