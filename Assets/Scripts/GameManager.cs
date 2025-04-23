using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Score")]
    private int score;
    private int highscore;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    [Header("UI & References")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private SaveScore saveScore = new SaveScore();

    private bool gameStarted = false;
    private ObstacleSpawner obstacleSpawner;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        scoreText.gameObject.SetActive(false);
        menuPanel.SetActive(true);

        highscore = saveScore.LoadScoreFromFile();
        highScoreText.text = "Highscore - " + highscore;

        AudioManager.Instance.FadeInMusic(1f);

        obstacleSpawner = GetComponent<ObstacleSpawner>();
    }

    private void Update()
    {
        if (!gameStarted)
        {
#if UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL || UNITY_EDITOR
            // Touch input for mobile and WebGL
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                StartGame();
            }
#endif

#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL
            // Keyboard input for desktop and WebGL
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
        scoreText.gameObject.SetActive(true);
        menuPanel.SetActive(false);

        obstacleSpawner.StartSpawning();
    }

    public void Restart()
    {
        if (score > highscore)
            saveScore.SaveScoreToFile(score);

        gameStarted = false;
        AudioManager.Instance.FadeOutMusic(3f);
        StartCoroutine(RestartAfterDelay());
    }

    private IEnumerator RestartAfterDelay()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(4f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ScoreUp()
    {
        score++;
        scoreText.text = score.ToString();

        if (score > highscore)
        {
            highscore = score;
            highScoreText.text = "Highscore - " + highscore;
        }
    }

    public bool IsGameRunning()
    {
        return gameStarted;
    }

    public float GetCurrentSpeed() => obstacleSpawner.GetCurrentSpeed();
}
