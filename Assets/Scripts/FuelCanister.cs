using UnityEngine;

public class FuelCanister : MonoBehaviour
{
    [SerializeField] private float refillAmount = 50f;
    [SerializeField] private GameObject pickupEffect; // Optional VFX

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandlePickup(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePickup(collision.gameObject);
    }

    private void HandlePickup(GameObject other)
    {
        // Debug exactly what hit us
        Debug.Log("<color=yellow>Fuel Canister touched by: </color>" + other.name);

        // 1. Find the Fuel System (in parent or singleton)
        FuelSystem playerFuel = other.GetComponentInParent<FuelSystem>();
        
        // If we didn't find it on the hit object, check the Singleton
        if (playerFuel == null) playerFuel = FuelSystem.Instance;

        if (playerFuel != null)
        {
            // 2. FOOLPROOF CHECK: If it has FuelSystem or PlayerHealth or DriveCar in parents, it's the car!
            bool isPlayerPart = other.GetComponentInParent<FuelSystem>() != null || 
                                other.GetComponentInParent<PlayerHealth>() != null ||
                                other.CompareTag("Player");

            if (isPlayerPart)
            {
                playerFuel.RefillFull();
                
                // Add score for fuel pickup
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScore(25);
                }

                if (pickupEffect != null)
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);

                Debug.Log("<color=green>SUCCESS: Fuel Refilled to 100%!</color>");
                Destroy(gameObject); // Disappear
            }
            else
            {
                Debug.Log("Fuel touched by non-player: " + other.name);
            }
        }
        else
        {
            Debug.LogError("<color=red>ERROR: Could not find FuelSystem! </color> " +
                           "Make sure the FuelSystem script is on your VEHICLE root.");
        }
    }
}
