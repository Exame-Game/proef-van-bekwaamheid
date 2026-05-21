using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [SerializeField] private float _minIntensity = 0.5f;
    [SerializeField] private float _maxIntensity = 1f;
    [SerializeField] private float _speed = 1f;

    private Light _light;

    private void Awake()
    {
        _light = GetComponent<Light>();
    }

    private void Update()
    {
        _light.intensity = Mathf.Lerp(_minIntensity, _maxIntensity, (Mathf.Sin(Time.time * _speed) + 1f) * 0.5f);
    }
}