using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("UI Panels")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject hudCanvas;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Game Over Stats")]
    [SerializeField] private TMPro.TextMeshProUGUI finalScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI finalTimeText;
    [SerializeField] private TMPro.TextMeshProUGUI finalEnemiesText;

    [Header("UI Components")]
    [SerializeField] private UnityEngine.UI.Slider loadingBar;

    [Header("Settings")]
    [SerializeField] private float loadingTime = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // Setup initial music
        if (musicSource != null && menuMusic != null)
        {
            musicSource.clip = menuMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        // Initial state
        if (loadingPanel != null) loadingPanel.SetActive(true);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (hudCanvas != null) hudCanvas.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (loadingBar != null) loadingBar.value = 0;

        // Pause the game world
        Time.timeScale = 0;

        StartCoroutine(LoadingSequence());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }

    private IEnumerator LoadingSequence()
    {
        float timer = 0;
        while (timer < loadingTime)
        {
            timer += Time.unscaledDeltaTime;
            if (loadingBar != null)
            {
                loadingBar.value = timer / loadingTime;
            }
            yield return null;
        }

        if (loadingPanel != null) loadingPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void PlayGame()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (hudCanvas != null) hudCanvas.SetActive(true);

        // Switch to gameplay music
        if (musicSource != null && gameplayMusic != null)
        {
            musicSource.Stop();
            musicSource.clip = gameplayMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        // Start the game world
        Time.timeScale = 1;
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (hudCanvas != null) hudCanvas.SetActive(false);
            
            Time.timeScale = 0;

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.SetGameOver();
                var stats = ScoreManager.Instance.GetFinalStats();
                
                if (finalScoreText != null) finalScoreText.text = stats.score.ToString();
                
                // Format time as MM:SS
                int minutes = Mathf.FloorToInt(stats.time / 60);
                int seconds = Mathf.FloorToInt(stats.time % 60);
                if (finalTimeText != null) finalTimeText.text = $"{minutes:D2}:{seconds:D2}";
                
                if (finalEnemiesText != null) finalEnemiesText.text = stats.enemies.ToString();
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void GoToHome()
    {
        Time.timeScale = 1; // Reset time scale
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit!");
    }
}
