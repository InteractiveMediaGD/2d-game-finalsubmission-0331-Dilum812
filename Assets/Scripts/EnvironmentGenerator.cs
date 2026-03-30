using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[ExecuteInEditMode]
public class EnvironmentGenerator : MonoBehaviour
{
    [SerializeField] private SpriteShapeController _spriteShapeController;

    [Range(3, 500)] public int _levelLength = 50;

    [Header("Hill Climb Generation")]
    [Tooltip("Unique seed. Change this for a totally different map!")]
    public float _terrainSeed = 0f;
    
    [Tooltip("Number of detail layers (Octaves). Higher = more 'rapid' changes (Trial: 3-5)")]
    [Range(1, 8)] public int _octaves = 4;
    
    [Tooltip("Scale of the largest hills. Lower = wider hills, Higher = more frequent peaks (Trial: 30-60)")]
    [Range(10f, 200f)] public float _baseFrequency = 40f;
    
    [Tooltip("Overall height of the hills (Trial: 15-40)")]
    [Range(1f, 100f)] public float _totalHeight = 25f;

    [Tooltip("How much detail each layer adds. (Trial: 0.4-0.6)")]
    [Range(0f, 1f)] public float _persistence = 0.5f;
    
    [Tooltip("How much frequency increases per layer. (Trial: 1.8-2.2)")]
    [Range(1f, 4f)] public float _lacunarity = 2.0f;

    [Tooltip("Makes hills steeper or flatter. (Trial: 1.0-2.5)")]
    [Range(0.1f, 5f)] public float _steepness = 1.0f;

    [Header("Visuals (Grass & Performance)")]
    [Tooltip("Horizontal distance between points. Increase for smoother roads, decrease for 'rapid' changes.")]
    [Range(1f, 20f)] public float _xMultiplier = 2.5f;
    [Tooltip("Smoothing of the curves. (Trial: 0.4-0.6)")]
    [Range(0f, 1f)] public float _curveSmoothness = 0.5f;
    [Tooltip("How deep the ground texture goes down.")]
    public float _bottom = 15f;

    [Header("Enemy Spawning")]
    [SerializeField] private List<GameObject> _enemyPrefabs;
    [SerializeField, Range(0f, 1f)] private float _spawnChance = 0.1f;
    [SerializeField] private Transform _enemyContainer;

    [Header("Fuel Spawning")]
    [SerializeField] private GameObject _fuelPrefab;
    [SerializeField] private float _fuelInterval = 100f; // Spawn canister every X units
    [SerializeField] private float _fuelYOffset = 0.5f; // Height above ground
    private float _nextFuelX = 0f;

    [Header("Infinite Generation")]
    [SerializeField] private Transform _player;
    [SerializeField] private float _spawnThreshold = 20f; // Distance from end to trigger more generation
    [SerializeField] private float _killZoneY = -20f; // Height threshold to trigger Game Over

    private void Start()
    {
        if (Application.isPlaying)
        {
            if (_player == null) _player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (_player != null) _nextFuelX = _player.position.x + _fuelInterval;
            GenerateInitialLevel();
        }
    }

    private void Update()
    {
        if (!Application.isPlaying || _player == null) return;

        // 1. Kill Zone Check (Falling off the world)
        if (_player.position.y < _killZoneY)
        {
            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.ShowGameOver();
            }
        }

        // 2. "Ground is Over" -> Game Over Logic
        int lastSurfaceIndex = _spriteShapeController.spline.GetPointCount() - 3;
        if (lastSurfaceIndex < 0) return;

        Vector3 lastPointPos = _spriteShapeController.spline.GetPosition(lastSurfaceIndex);
        
        // If player passes the end of the ground, it's Game Over
        if (_player.position.x > lastPointPos.x)
        {
            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.ShowGameOver();
            }
        }
    }

    private void GenerateInitialLevel()
    {
        _spriteShapeController.spline.Clear();

        for (int i = 0; i < _levelLength; i++)
        {
            AddSplinePoint(i);
        }
    }

    private void AddSplinePoint(int index)
    {
        // 1. Fractal Noise Logic for "Rapid Changes"
        float xDist = index * _xMultiplier;
        float y = 0;
        float frequency = 1f / _baseFrequency;
        float amplitude = 1f;
        float maxValue = 0f;

        for (int i = 0; i < _octaves; i++)
        {
            float noiseValue = Mathf.PerlinNoise(_terrainSeed + (i * 100), xDist * frequency);
            y += noiseValue * amplitude;
            maxValue += amplitude;
            
            frequency *= _lacunarity;
            amplitude *= _persistence;
        }

        // Normalize and Scale
        y = (y / maxValue) * _totalHeight;
        
        // Apply Steepness (Power function makes peaks sharper and valleys flatter)
        y = Mathf.Pow(y / _totalHeight, _steepness) * _totalHeight;

        Vector3 newPos = transform.position + new Vector3(xDist, y, 0);

        // 2. Safety Check: Avoid "Point too close to neighbor" error
        int totalPoints = _spriteShapeController.spline.GetPointCount();
        if (totalPoints > 0)
        {
            int prevIndex = index - 1;
            if (prevIndex >= 0 && prevIndex < totalPoints)
            {
                float dist = Vector3.Distance(_spriteShapeController.spline.GetPosition(prevIndex), newPos);
                if (dist < 0.5f) return; 
            }
        }

        // 3. Management of Surface and Bottom points
        if (totalPoints < 3)
        {
            _spriteShapeController.spline.InsertPointAt(index, newPos);
            // Linear edge for first point (Fixes Grass)
            _spriteShapeController.spline.SetTangentMode(index, ShapeTangentMode.Linear);
            
            _spriteShapeController.spline.InsertPointAt(index + 1, new Vector3(newPos.x + 0.5f, transform.position.y - _bottom));
            _spriteShapeController.spline.InsertPointAt(index + 2, new Vector3(transform.position.x - 0.5f, transform.position.y - _bottom));
        }
        else
        {
            _spriteShapeController.spline.InsertPointAt(index, newPos);
            _spriteShapeController.spline.SetPosition(index + 1, new Vector3(newPos.x, transform.position.y - _bottom));
            _spriteShapeController.spline.SetPosition(index + 2, new Vector3(transform.position.x, transform.position.y - _bottom));
        }

        // 4. Smooth the surface tangents - SLOPE-ALIGNED & CLAMPED (Fixes Grass Hide & Tessellation)
        if (index > 0)
        {
            int prevIndex = index - 1;
            // Only smooth points that are not the first or last surface point
            if (prevIndex > 0 && prevIndex < _spriteShapeController.spline.GetPointCount() - 2)
            {
                _spriteShapeController.spline.SetTangentMode(prevIndex, ShapeTangentMode.Continuous);
                
                // Calculate Slope-Aligned Tangent Direction
                // This prevents self-intersection on steep hills
                Vector3 pLeft = _spriteShapeController.spline.GetPosition(prevIndex - 1);
                Vector3 pRight = _spriteShapeController.spline.GetPosition(index); // This is the new point
                Vector3 tangentDir = (pRight - pLeft).normalized;
                
                float maxTangentLength = (_xMultiplier * 0.45f) * _curveSmoothness;
                _spriteShapeController.spline.SetLeftTangent(prevIndex, -tangentDir * maxTangentLength);
                _spriteShapeController.spline.SetRightTangent(prevIndex, tangentDir * maxTangentLength);
            }

            // Keep edges linear to avoid bottom-corner intersections
            _spriteShapeController.spline.SetTangentMode(index, ShapeTangentMode.Linear);
        }

        // 5. Update Collider and Spawn Fuel Canisters at fixed intervals
        if (Application.isPlaying && _fuelPrefab != null)
        {
            _spriteShapeController.BakeCollider();
            if (newPos.x >= _nextFuelX && newPos.x > 5f)
            {
                SpawnFuel(newPos);
                _nextFuelX += _fuelInterval;
            }
        }
    }

    private void SpawnFuel(Vector3 groundWorldPos)
    {
        Transform container = transform.Find("FuelContainer");
        if (container == null) container = new GameObject("FuelContainer").transform;
        container.SetParent(transform);

        Vector3 rayStart = groundWorldPos + Vector3.up * 10f;
        RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, 20f);
        
        Vector3 finalPos = groundWorldPos;
        if (hit.collider != null) finalPos = hit.point;

        GameObject fuel = Instantiate(_fuelPrefab, finalPos, Quaternion.identity, container);
        fuel.name = "FuelCanister_Near_" + Mathf.Round(finalPos.x);

        Rigidbody2D fuelRb = fuel.GetComponent<Rigidbody2D>();
        if (fuelRb != null)
        {
            fuelRb.bodyType = RigidbodyType2D.Kinematic;
            fuelRb.linearVelocity = Vector2.zero;
        }

        Collider2D[] colliders = fuel.GetComponentsInChildren<Collider2D>();
        if (colliders.Length > 0)
        {
            float minY = float.MaxValue;
            foreach (Collider2D col in colliders) minY = Mathf.Min(minY, col.bounds.min.y);
            float verticalOffset = fuel.transform.position.y - minY;
            fuel.transform.position += new Vector3(0, verticalOffset + _fuelYOffset, 0);
        }
        else
        {
            fuel.transform.position += new Vector3(0, _fuelYOffset, 0);
        }
    }

    public void OnValidate()
    {
        if (_spriteShapeController == null) return;
        GenerateInitialLevel();
    }

    [ContextMenu("Spawn Enemies")]
    public void SpawnEnemies()
    {
        if (_enemyContainer == null)
        {
            _enemyContainer = new GameObject("EnemyContainer").transform;
            _enemyContainer.parent = transform;
        }

        for (int i = _enemyContainer.childCount - 1; i >= 0; i--)
        {
            GameObject child = _enemyContainer.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child);
            else if (child.scene.name != null) DestroyImmediate(child);
        }

        if (_enemyPrefabs == null || _enemyPrefabs.Count == 0) return;

        for (int i = 1; i < _spriteShapeController.spline.GetPointCount() - 3; i++)
        {
            if (Random.value < _spawnChance)
            {
                Vector3 spawnPos = _spriteShapeController.spline.GetPosition(i) + transform.position;
                GameObject prefab = _enemyPrefabs[Random.Range(0, _enemyPrefabs.Count)];
                GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity, _enemyContainer);
                enemy.name = "Enemy_" + i;
                enemy.transform.position += new Vector3(0, 2f, -5f);
            }
        }
    }
}