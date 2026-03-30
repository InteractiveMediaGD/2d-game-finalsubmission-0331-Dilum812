using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    [Header("Scoring Settings")]
    [SerializeField] private float pointsPerMeter = 1f;

    private int _currentScore = 0;
    private int _enemiesDestroyed = 0;
    private float _timeSurvived = 0f;
    private bool _isGameOver = false;

    private float _maxDistance = 0f;
    private float _accumulatedDistanceScore = 0f;
    private Transform _playerTransform;
    private Vector3 _startPosition;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _timeSurvived = 0f;
        _isGameOver = false;
        
        DriveCar playerScript = Object.FindFirstObjectByType<DriveCar>();
        if (playerScript != null)
        {
            _playerTransform = playerScript.transform;
            _startPosition = _playerTransform.position;
        }
        
        UpdateScoreUI();
    }

    private void Update()
    {
        if (_isGameOver) return;

        _timeSurvived += Time.deltaTime;

        if (_playerTransform != null)
        {
            float currentDistance = Mathf.Abs(_playerTransform.position.x - _startPosition.x);
            
            if (currentDistance > _maxDistance)
            {
                float distanceGain = currentDistance - _maxDistance;
                _maxDistance = currentDistance;
                
                _accumulatedDistanceScore += distanceGain * pointsPerMeter;

                if (_accumulatedDistanceScore >= 1f)
                {
                    int pointsToAdd = Mathf.FloorToInt(_accumulatedDistanceScore);
                    _currentScore += pointsToAdd;
                    _accumulatedDistanceScore -= pointsToAdd;
                    UpdateScoreUI();
                }
            }
        }
    }

    public void AddScore(int amount)
    {
        if (_isGameOver) return;
        _currentScore += amount;
        UpdateScoreUI();
    }

    public void OnEnemyDestroyed()
    {
        if (_isGameOver) return;
        _enemiesDestroyed++;
        // Optional: Add points for killing enemies
        AddScore(10);
    }

    public void SetGameOver()
    {
        _isGameOver = true;
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + _currentScore.ToString();
        }
    }

    public void ShowFinalScore()
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + _currentScore.ToString();
        }
    }

    public (int score, int enemies, float time) GetFinalStats()
    {
        return (_currentScore, _enemiesDestroyed, _timeSurvived);
    }
}
