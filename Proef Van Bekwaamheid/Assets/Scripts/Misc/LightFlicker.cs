using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public float _minIntensity = 0.5f;
    public float _maxIntensity = 1f;
    public float _speed = 1f;

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