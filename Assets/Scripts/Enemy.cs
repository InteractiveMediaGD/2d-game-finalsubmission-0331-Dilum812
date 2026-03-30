using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private EnemyHealthBar healthBar;
    private float currentHealth;
    private bool healthBarVisible = false;

    private void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth);
            healthBar.SetVisible(false);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log("Enemy hit! Health remaining: " + currentHealth);

        if (healthBar != null)
        {
            if (!healthBarVisible)
            {
                healthBar.SetVisible(true);
                healthBarVisible = true;
            }
            healthBar.UpdateHealth(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.OnEnemyDestroyed();
        
        Debug.Log("Enemy Destroyed!");
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If enemy touches player car directly (optional requirement from lecturer)
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name.Contains("VEHICLE"))
        {
            // You can add logic here to hurt the player
            // PlayerHealth.instance.TakeDamage(10); 
            // Destroy(gameObject); // Enemy dies on impact
        }
    }
}
