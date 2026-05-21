using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class DayNightCycle : MonoBehaviour
{
    [Header("Sun")]
    [SerializeField] private Gradient _sunColor;
    [SerializeField] private AnimationCurve _sunIntensity;

    [Header("Ambient")]
    [SerializeField] private Gradient _ambientColor;

    private Light _sun;
    
    private void Awake()
    {
        _sun = GetComponent<Light>();
        InitializeDefaults();
    }

    public void ApplyCycle(float t)
    {

        float shifted = (t + 0.5f) % 1f;
        // 0 = midnight, 0.25 = sunrise, 0.5 = noon, 0.75 = sunset
        transform.localRotation = Quaternion.Euler((shifted * 360f) - 90f, 170f, 0f);

        _sun.color = _sunColor.Evaluate(shifted);
        _sun.intensity = _sunIntensity.Evaluate(shifted + .2f);

        RenderSettings.ambientLight = _ambientColor.Evaluate(shifted);

        DynamicGI.UpdateEnvironment();
    }

    private void InitializeDefaults()
    {
        if (_sunColor.colorKeys.Length == 0)
            _sunColor = BuildDefaultSunColor();

        if (_sunIntensity.keys.Length == 0)
            _sunIntensity = BuildDefaultSunIntensity();

        if (_ambientColor.colorKeys.Length == 0)
            _ambientColor = BuildDefaultAmbientColor();

    }

    private static Gradient BuildDefaultSunColor()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new(Color.black,                   0.00f),
                new(new Color(1f, 0.45f, 0.1f),   0.23f),
                new(new Color(1f, 0.90f, 0.7f),   0.27f),
                new(Color.white,                   0.50f),
                new(new Color(1f, 0.90f, 0.7f),   0.73f),
                new(new Color(1f, 0.45f, 0.1f),   0.77f),
                new(Color.black,                   1.00f),
            },
            new GradientAlphaKey[] { new(1f, 0f), new(1f, 1f) }
        );

        return gradient;
    }

    private static AnimationCurve BuildDefaultSunIntensity()
    {
        return new AnimationCurve(
            new Keyframe(0.00f, 0f),
            new Keyframe(0.25f, 0f),
            new Keyframe(0.30f, 0.8f),
            new Keyframe(0.50f, 1.2f),
            new Keyframe(0.70f, 0.8f),
            new Keyframe(0.75f, 0f),
            new Keyframe(1.00f, 0f)
        );
    }

    private static Gradient BuildDefaultAmbientColor()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new(new Color(0.05f, 0.05f, 0.10f), 0.00f),
                new(new Color(0.40f, 0.30f, 0.60f), 0.24f),
                new(new Color(0.50f, 0.70f, 1.00f), 0.50f),
                new(new Color(0.40f, 0.30f, 0.60f), 0.76f),
                new(new Color(0.05f, 0.05f, 0.10f), 1.00f),
            },
            new GradientAlphaKey[] { new(1f, 0f), new(1f, 1f) }
        );

        return gradient;
    }
}
