using System.Collections.Generic;
using UnityEngine;

public class AlarmMode : MonoBehaviour
{
    [SerializeField] private List<Light> _lights;
    [SerializeField] private List<Light> _staticAlarmLights;

    private struct LightData
    {
        public Color color;
        public bool enabled;
        public float maxIntensity;
        public bool hadFlicker;
        public float minIntensity;
        public float speed;
    }

    private Dictionary<Light, LightData> _originalStates = new Dictionary<Light, LightData>();

    private void Awake()
    {
        SaveOriginalState();
    }

    private void SaveOriginalState()
    {
        foreach (Light light in _lights)
        {
            LightFlicker existingFlicker = light.GetComponent<LightFlicker>();
            _originalStates[light] = new LightData
            {
                color = light.color,
                enabled = light.enabled,
                maxIntensity = light.intensity,
                hadFlicker = existingFlicker != null,
                minIntensity = existingFlicker != null ? existingFlicker._minIntensity : 10f,
                speed = existingFlicker != null ? existingFlicker._speed : 1f,
            };
        }

        foreach (Light light in _staticAlarmLights)
            _originalStates[light] = new LightData
            {
                color = light.color,
                enabled = light.enabled,
                maxIntensity = light.intensity,
            };
    }

    public void ActivateAlarmMode()
    {
        foreach (Light light in _lights)
        {
            LightData data = _originalStates[light];

            LightFlicker flicker = light.GetComponent<LightFlicker>();
            if (flicker == null)
                flicker = light.gameObject.AddComponent<LightFlicker>();

            flicker._speed = 10f;
            flicker._maxIntensity = data.maxIntensity;
            flicker._minIntensity = 5f;
            light.color = Color.red;
        }

        foreach (Light light in _staticAlarmLights)
        {
            light.color = Color.red;
            light.enabled = true;
        }
    }

    public void DeactivateAlarmMode()
    {
        foreach (Light light in _lights)
        {
            LightData data = _originalStates[light];
            LightFlicker flicker = light.GetComponent<LightFlicker>();

            if (flicker != null)
                if (data.hadFlicker)
                {
                    flicker._speed = data.speed;
                    flicker._maxIntensity = data.maxIntensity;
                    flicker._minIntensity = data.minIntensity;
                }
                else
                {
                    flicker.enabled = false;
                    Destroy(flicker);
                }

            light.intensity = data.maxIntensity;
            light.color = data.color;
            light.enabled = data.enabled;
        }

        foreach (Light light in _staticAlarmLights)
            if (_originalStates.TryGetValue(light, out LightData data))
            {
                light.intensity = data.maxIntensity;
                light.color = data.color;
                light.enabled = data.enabled;
            }
    }
}