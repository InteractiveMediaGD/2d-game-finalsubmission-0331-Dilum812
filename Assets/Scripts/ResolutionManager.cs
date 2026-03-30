using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ResolutionManager : MonoBehaviour
{
    public Vector2 targetResolution = new Vector2(1920, 1080);

    [ContextMenu("Update Game Resolution")]
    public void UpdateResolution()
    {
        CanvasScaler[] scalers = FindObjectsByType<CanvasScaler>(FindObjectsSortMode.None);
        foreach (CanvasScaler scaler in scalers)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = targetResolution;
            scaler.matchWidthOrHeight = 0.5f; // Balance between width and height scaling
            Debug.Log($"[ResolutionManager] Updated {scaler.gameObject.name} to {targetResolution}");
        }
    }

    private void Start()
    {
        if (Application.isPlaying) UpdateResolution();
    }
}
