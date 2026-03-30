using UnityEngine;
using UnityEngine.UI;

public class ScreenWarningOverlay : MonoBehaviour
{
    public static ScreenWarningOverlay Instance;

    [SerializeField] private Image warningImage;
    [SerializeField] private float pulseSpeed = 10f;
    [SerializeField] private float maxAlpha = 0.5f;

    private bool isHealthCritical = false;
    private bool isFuelCritical = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        if (warningImage != null)
        {
            // Ensure it starts invisible
            Color c = warningImage.color;
            c.a = 0;
            warningImage.color = c;
            warningImage.gameObject.SetActive(true);
        }
    }

    public void SetHealthCritical(bool critical)
    {
        isHealthCritical = critical;
    }

    public void SetFuelCritical(bool critical)
    {
        isFuelCritical = critical;
    }

    private void Update()
    {
        if (warningImage == null) return;

        bool isAnyCritical = isHealthCritical || isFuelCritical;

        if (isAnyCritical)
        {
            // Create a "Blink" effect using a sharp pulse (Sin squared or similar)
            // This gives it a more rhythmic, urgent "warning" feel than a linear PingPong
            float blink = Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed));
            
            Color c = warningImage.color;
            c.a = blink * maxAlpha;
            warningImage.color = c;
        }
        else
        {
            // Smoothly fade out if previously active
            if (warningImage.color.a > 0.01f)
            {
                Color c = warningImage.color;
                c.a = Mathf.Lerp(c.a, 0, Time.deltaTime * pulseSpeed);
                warningImage.color = c;
            }
            else if (warningImage.color.a != 0)
            {
                Color c = warningImage.color;
                c.a = 0;
                warningImage.color = c;
            }
        }
    }
}
