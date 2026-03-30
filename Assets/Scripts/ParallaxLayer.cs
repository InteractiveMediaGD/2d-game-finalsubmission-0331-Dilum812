using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Tooltip("Move factor relative to the camera. 0.0 = static (Sky), 1.0 = moves exactly with camera (Ground).")]
    [Range(0f, 1f)] public float parallaxFactor = 0.5f;

    private Transform _cameraTransform;
    private Vector3 _lastCameraPosition;
    private float _textureUnitSizeX;

    private float _startpos;

    private void Start()
    {
        _cameraTransform = Camera.main.transform;
        _startpos = transform.position.x;
        
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            _textureUnitSizeX = sr.sprite.bounds.size.x;
        }
    }

    private void LateUpdate()
    {
        float temp = (_cameraTransform.position.x * (1 - parallaxFactor));
        float dist = (_cameraTransform.position.x * parallaxFactor);

        transform.position = new Vector3(_startpos + dist, transform.position.y, transform.position.z);

        if (temp > _startpos + _textureUnitSizeX) _startpos += _textureUnitSizeX;
        else if (temp < _startpos - _textureUnitSizeX) _startpos -= _textureUnitSizeX;
    }
}
