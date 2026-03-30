using UnityEngine;
using UnityEngine.UI;

public class FuelSystem : MonoBehaviour
{
    public static FuelSystem Instance;

    [Header("Fuel Settings")]
    [SerializeField] private float maxFuel = 100f;
    
    [SerializeField, Tooltip("Base fuel loss per second (Lower = longer life)")] 
    private float fuelConsumptionRate = 1f; 
    
    [SerializeField, Tooltip("Extra fuel loss when holding gas (Lower = more efficient acceleration)")] 
    private float accelerationBurnRate = 2f; 

    [Header("UI")]
    [SerializeField] private Image fuelGauge; // Link your UI Image (Fill Amount)
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Gradient _fuelGradient; // Professional color change

    private float _currentFuel;
    private Rigidbody2D _rb;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
             // Try to destroy the duplicate if it's on the same object or just warn
             Debug.LogWarning("Duplicate FuelSystem found and ignored.");
        }
        Instance = this;
        _rb = GetComponent<Rigidbody2D>();
        
        // Initialize early!
        _currentFuel = maxFuel; 
    }

    private void Update()
    {
        if (_currentFuel <= 0) 
        {
            // Even if empty, update UI color
            UpdateUI();
            return;
        }

        // 1. Calculate consumption
        float burnAmount = fuelConsumptionRate * Time.deltaTime;

        // 2. Engaging mechanic: Burn more if accelerating/climbing
        // We check horizontal input or torque to see if the engine is working hard
        float input = Mathf.Abs(Input.GetAxis("Horizontal"));
        if (input > 0.1f)
        {
            burnAmount += accelerationBurnRate * input * Time.deltaTime;
        }

        _currentFuel -= burnAmount;
        _currentFuel = Mathf.Clamp(_currentFuel, 0, maxFuel);

        float fuelPercent = _currentFuel / maxFuel;

        // Trigger Screen Warning Effect (10%)
        if (ScreenWarningOverlay.Instance != null)
        {
            ScreenWarningOverlay.Instance.SetFuelCritical(fuelPercent < 0.1f);
        }

        UpdateUI();

        if (_currentFuel <= 0)
        {
            OutOfFuel();
        }
    }

    public void Refill(float amount)
    {
        _currentFuel += amount;
        _currentFuel = Mathf.Clamp(_currentFuel, 0, maxFuel);
        UpdateUI();
        Debug.Log("Fuel Refilled! Current: " + _currentFuel);
    }

    public void RefillFull()
    {
        _currentFuel = maxFuel;
        UpdateUI();
        Debug.Log("<color=green>Fuel Refilled to 100% on: </color>" + gameObject.name);
    }

    private Color _originalColor;

    private void Start()
    {
        _currentFuel = maxFuel;
        
        // 1. SILENCE OLD ERRORS
        FuelController oldController = UnityEngine.Object.FindFirstObjectByType<FuelController>();
        if (oldController != null)
        {
            oldController.enabled = false;
        }

        // 2. Auto-Find Logic
        if (fuelGauge == null || fuelGauge.gameObject.scene.name == null)
        {
            GameObject foundUI = GameObject.Find("FuelFront");
            if (foundUI != null) fuelGauge = foundUI.GetComponent<Image>();
        }

        if (fuelGauge != null)
        {
            _originalColor = Color.green; // FORCE GREEN AS DEFAULT
            fuelGauge.color = Color.green;
            Debug.Log("<color=magenta>FuelSystem LINKED TO: " + fuelGauge.name + " in " + fuelGauge.gameObject.scene.name + "</color>");
        }
        else
        {
            Debug.LogError("<color=red>FuelSystem: NO UI FOUND! Please name your image 'FuelFront'</color>");
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (fuelGauge != null)
        {
            float targetFill = _currentFuel / maxFuel;
            fuelGauge.fillAmount = targetFill;
            
            // Pulsing effect for critical fuel (consistent with health bar)
            if (targetFill < 0.2f)
            {
                float pulse = Mathf.PingPong(Time.time * 5f, 1f);
                fuelGauge.color = Color.Lerp(Color.red, Color.black, pulse);
            }
            else
            {
                // Standardized Gradient Logic
                if (_fuelGradient != null && _fuelGradient.colorKeys.Length > 0)
                {
                    fuelGauge.color = _fuelGradient.Evaluate(targetFill);
                }
                else
                {
                    // Fallback Code-Based Gradient (Matching Health Bar)
                    if (targetFill > 0.5f)
                    {
                        float t = (targetFill - 0.5f) * 2f;
                        fuelGauge.color = Color.Lerp(Color.yellow, Color.green, t);
                    }
                    else
                    {
                        float t = targetFill / 0.5f;
                        fuelGauge.color = Color.Lerp(Color.red, Color.yellow, t);
                    }
                }
            }
        }
    }

    private void OutOfFuel()
    {
        Debug.Log("Out of Fuel! Game Over.");
        
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.ShowGameOver();
        }
        else
        {
            // Fallback: Reload the current active scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
