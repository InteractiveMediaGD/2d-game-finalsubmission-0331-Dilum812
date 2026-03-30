using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float playerDamage = 10f;
    [SerializeField] private float enemyDamage = 5f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float gravityScale = 0.5f; // New: Bullet drop
    public bool isEnemyProjectile = false;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale; // Apply gravity
        
        // Force direction based on team to avoid issues with flipped parent scales.
        // Enemy bullets go Left (-1), Player bullets go Right (1).
        float moveDirection = isEnemyProjectile ? -1f : 1f;
        rb.linearVelocity = new Vector2(moveDirection * speed, 0);
        
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        // Align rotation with velocity for a natural "flight" look
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Identify the hit target
        PlayerHealth player = collision.GetComponentInParent<PlayerHealth>();
        Enemy enemy = collision.GetComponentInParent<Enemy>();

        // 2. Handle Enemy Projectiles (Bullets fired by enemies)
        if (isEnemyProjectile)
        {
            // Ignore hitting own side
            if (enemy != null) return;

            // Use singleton for speed!
            if (player == null)
            {
                player = PlayerHealth.Instance;
            }
            
            if (player != null)
            {
                // Standard log now that we've confirmed it works!
                Debug.Log($"[Projectile] HIT PLAYER! Dealing {enemyDamage} damage to {player.gameObject.name}");
                player.TakeDamage(enemyDamage);
                Destroy(gameObject);
                return;
            }
        }
        // 3. Handle Player Projectiles
        else
        {
            if (player != null) return; // Ignore own side
            
            if (enemy != null)
            {
                Debug.Log($"[Projectile] HIT ENEMY! Dealing {playerDamage} damage to {enemy.gameObject.name}");
                enemy.TakeDamage(playerDamage);
                Destroy(gameObject);
                return;
            }
        }

        // 4. Destroy on impact with ground or obstacles (non-trigger colliders)
        if (!collision.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
