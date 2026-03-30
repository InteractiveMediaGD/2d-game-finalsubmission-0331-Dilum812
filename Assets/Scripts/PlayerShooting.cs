using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    
    [Header("Audio")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioSource audioSource;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            // Only block shooting if we are in the Menu (Time is stopped)
            // This prevents "auto-shoot" when clicking Play.
            if (Time.timeScale == 0)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
            }

            Shoot();
        }
    }

    private void Shoot()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        }

        // Play shoot sound if both clip and source are assigned
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }
}
