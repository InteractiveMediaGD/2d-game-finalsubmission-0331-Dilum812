using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient healthGradient;
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);

    private Quaternion initialRotation;

    private void Start()
    {
        initialRotation = transform.rotation;
        
        // Find the main camera automatically for spawned enemies
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        // Keep the health bar horizontal and above the enemy
        transform.rotation = Quaternion.identity; 
        if (transform.parent != null)
        {
            transform.position = transform.parent.position + offset;
        }
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (fillImage == null) return;

        float targetFill = currentHealth / maxHealth;
        fillImage.fillAmount = targetFill;

        // Use the gradient to set the color
        if (healthGradient != null)
        {
            fillImage.color = healthGradient.Evaluate(targetFill);
        }
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
