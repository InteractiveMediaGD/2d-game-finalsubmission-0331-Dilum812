using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootInterval = 2.5f;

    private float timer;
    private Transform player;

    private void Start()
    {
        timer = shootInterval;
        GameObject playerObj = GameObject.Find("VEHICLE");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Only shoot if player is in range (optional)
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance < 25f) 
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                Shoot();
                timer = shootInterval;
            }
        }
    }

    private void Shoot()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            
            // Mark this bullet as an enemy projectile so it hurts the player!
            Projectile p = bullet.GetComponent<Projectile>();
            if (p != null)
            {
                p.isEnemyProjectile = true;
            }
        }
    }
}
