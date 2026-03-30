using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicEnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _player;
    [SerializeField] private List<GameObject> _enemyPrefabs;
    [SerializeField] private GameObject _fuelPrefab;
    [SerializeField] private Transform _enemyContainer;

    [Header("Spawn Settings")]
    [SerializeField] private float _spawnAheadDistance = 100f;
    [SerializeField, Tooltip("Minimum distance between spawns (Increase this for fewer enemies)")] 
    private float _minDistanceBetween = 100f; // Increased for fewer enemies
    
    [SerializeField, Tooltip("Maximum distance between spawns (Increase this for fewer enemies)")] 
    private float _maxDistanceBetween = 250f; // Increased for fewer enemies
    [SerializeField, Tooltip("Chance (0 to 1) that fuel spawns instead of an enemy. Increase this for more fuel!")] 
    private float _fuelSpawnChance = 0.3f; 
    [SerializeField] private float _yOffset = 0.5f;

    [SerializeField, Tooltip("Minimum distance to existing fuel before spawning another.")]
    private float _minFuelDistance = 20f;
    
    [Header("Terrain Settings")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _raycastHeight = 20f;

    private float _nextSpawnX;

    private void Start()
    {
        // Safety Check: Ensure container is a scene object, not a prefab
        if (_enemyContainer != null && _enemyContainer.gameObject.scene.name == null)
        {
            Debug.LogWarning("DynamicEnemySpawner: Enemy Container was assigned to a Prefab! Using a new container instead.");
            _enemyContainer = null;
        }

        if (_player == null)
        {
            _player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (_player == null)
            {
                Debug.LogWarning("DynamicEnemySpawner: Player not found! Please assign it in the Inspector.");
            }
        }

        if (_enemyContainer == null)
        {
            _enemyContainer = new GameObject("DynamicEnemies").transform;
        }

        // Initialize first spawn point ahead of player
        if (_player != null)
        {
            _nextSpawnX = _player.position.x + _spawnAheadDistance + _minDistanceBetween; 
        }
    }

    private void Update()
    {
        if (_player == null) return;

        // Check if player is approaching the next spawn point AND there is no current enemy/fuel in the area
        if (_player.position.x > _nextSpawnX - _spawnAheadDistance && _enemyContainer.childCount == 0)
        {
            bool shouldSpawnFuel = Random.value < _fuelSpawnChance && _fuelPrefab != null;

            if (shouldSpawnFuel && IsFuelNearby())
            {
                Debug.Log("DynamicEnemySpawner: Skipping fuel spawn due to nearby existing fuel.");
                SpawnEnemy(); // Better to spawn an enemy than nothing, or just skip
            }
            else if (shouldSpawnFuel)
            {
                SpawnFuel();
            }
            else
            {
                SpawnEnemy();
            }
            
            // Move the next spawn point further ahead of the player's current position
            _nextSpawnX = _player.position.x + Random.Range(_minDistanceBetween, _maxDistanceBetween) + _spawnAheadDistance;
        }

        // Optional: Clean up enemies far behind the player (e.g., 50 units behind)
        CleanupEnemies();
    }

    private bool IsFuelNearby()
    {
        // Search for any existing FuelCanister in the world
        FuelCanister[] existingFuel = Object.FindObjectsByType<FuelCanister>(FindObjectsSortMode.None);
        foreach (FuelCanister fuel in existingFuel)
        {
            if (Mathf.Abs(fuel.transform.position.x - _nextSpawnX) < _minFuelDistance)
            {
                return true;
            }
        }
        return false;
    }

    private void SpawnEnemy()
    {
        if (_enemyPrefabs == null || _enemyPrefabs.Count == 0)
        {
            Debug.LogError("DynamicEnemySpawner: Enemy Prefabs list is EMPTY! Please drag an enemy prefab into the list in the Inspector.");
            return;
        }

        // Find ground height at _nextSpawnX
        Vector2 rayOrigin = new Vector2(_nextSpawnX, _player.position.y + _raycastHeight);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, _raycastHeight * 2f, _groundLayer);

        if (hit.collider != null)
        {
            Vector3 spawnPos = hit.point;
            spawnPos.y += 0.1f; // Tiny buffer to avoid spawning inside ground
            
            // Calculate rotation to match ground slope
            Quaternion spawnRotation = Quaternion.FromToRotation(Vector2.up, hit.normal);
            
            GameObject prefab = _enemyPrefabs[Random.Range(0, _enemyPrefabs.Count)];
            
            GameObject newEnemy = Instantiate(prefab, spawnPos, spawnRotation, _enemyContainer);
            
            // --- Smart Snap Logic ---
            // Adjust position so the bottom of the wheels touches the hit.point exactly
            Collider2D[] colliders = newEnemy.GetComponentsInChildren<Collider2D>();
            if (colliders.Length > 0)
            {
                float minY = float.MaxValue;
                foreach (Collider2D col in colliders)
                {
                    minY = Mathf.Min(minY, col.bounds.min.y);
                }
                
                float verticalOffset = newEnemy.transform.position.y - minY;
                newEnemy.transform.position += new Vector3(0, verticalOffset, 0);
                Debug.Log("DynamicEnemySpawner: Smart Snapped " + newEnemy.name + " with offset: " + verticalOffset);
            }

            Debug.Log("DynamicEnemySpawner: Spawned " + newEnemy.name + " at X: " + spawnPos.x + " with rotation: " + spawnRotation.eulerAngles.z);
        }
        else
        {
            Debug.LogWarning("DynamicEnemySpawner: Raycast failed to find ground for Enemy at X: " + _nextSpawnX);
        }
    }

    private void SpawnFuel()
    {
        // Find ground height at _nextSpawnX
        Vector2 rayOrigin = new Vector2(_nextSpawnX, _player.position.y + _raycastHeight);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, _raycastHeight * 2f, _groundLayer);

        if (hit.collider != null)
        {
            Vector3 spawnPos = hit.point;
            spawnPos.y += 0.5f; // Small buffer
            
            GameObject fuelObj = Instantiate(_fuelPrefab, spawnPos, Quaternion.identity, _enemyContainer);
            fuelObj.name = "FuelCanister";
            
            SnapToGround(fuelObj); // Exact placement
            
            Debug.Log("DynamicEnemySpawner: Spawned Fuel Canister snapped to ground at X: " + spawnPos.x);
        }
        else
        {
            Debug.LogWarning("DynamicEnemySpawner: Raycast failed to find ground for Fuel at X: " + _nextSpawnX);
        }
    }

    private void SnapToGround(GameObject obj)
    {
        Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
        if (colliders.Length > 0)
        {
            float minY = float.MaxValue;
            foreach (Collider2D col in colliders)
            {
                minY = Mathf.Min(minY, col.bounds.min.y);
            }
            float verticalOffset = obj.transform.position.y - minY;
            obj.transform.position += new Vector3(0, verticalOffset, 0);
        }
    }

    private void CleanupEnemies()
    {
        float cleanupDistance = 50f;
        for (int i = _enemyContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = _enemyContainer.GetChild(i);
            if (child.position.x < _player.position.x - cleanupDistance)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
