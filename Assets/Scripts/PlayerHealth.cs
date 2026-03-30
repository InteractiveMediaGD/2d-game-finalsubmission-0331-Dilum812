using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;

    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Image healthImage; // Link your UI Image here
    [SerializeField] private Gradient healthGradient; // Optional: Custom color control
    
    private float currentHealth;

    [SerializeField] private GameObject gameOverPanel; // Assign your Game Over Panel UI object here

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        // Time.timeScale = 1; // Removed: MenuManager now handles initial timeScale
    }

    private void Update()
    {
        float healthPercent = currentHealth / maxHealth;

        // Trigger Screen Warning Effect (10%)
        if (ScreenWarningOverlay.Instance != null)
        {
            ScreenWarningOverlay.Instance.SetHealthCritical(healthPercent < 0.1f);
        }

        // Add a pulsing effect if health is critical (20%)
        if (healthPercent < 0.2f && healthImage != null)
        {
            float t = Mathf.PingPong(Time.time * 5f, 1f);
            healthImage.color = Color.Lerp(Color.red, Color.black, t);
        }
        else if (healthImage != null)
        {
            UpdateUI(); // Ensure color is updated even if not taking damage (to reset flash)
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateUI()
    {
        if (healthImage != null)
        {
            float targetFill = currentHealth / maxHealth;
            healthImage.fillAmount = targetFill;

            if (targetFill >= 0.2f) // Don't override the critical flash color
            {
                if (healthGradient != null && healthGradient.colorKeys.Length > 0)
                {
                    healthImage.color = healthGradient.Evaluate(targetFill);
                }
                else
                {
                    // Fallback Code-Based Gradient
                    if (targetFill > 0.5f)
                    {
                        float t = (targetFill - 0.5f) * 2f;
                        healthImage.color = Color.Lerp(Color.yellow, Color.green, t);
                    }
                    else
                    {
                        float t = targetFill / 0.5f;
                        healthImage.color = Color.Lerp(Color.red, Color.yellow, t);
                    }
                }
            }
        }
    }

    private void Die()
    {
        Debug.Log("Game Over! Player Died.");
        
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.ShowGameOver();
        }
        else if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0;
        }
    }

    // Call this from a UIButton (On Click event)
    public void RestartGame()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
