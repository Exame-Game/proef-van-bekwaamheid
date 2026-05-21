using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(DayNightCycle))]

public class GameTimer : MonoBehaviour
{
    private const float k_DefaultTime = 20f;

    [SerializeField] private Image _hourglassImage;

    public float _totalTime = k_DefaultTime;
    public UnityEvent OnGameLost;

    private DayNightCycle _dayNightCycle;
    private float _timeRemaining;
    private bool _isGameOver;

    private void Start()
    {
        _dayNightCycle = GetComponent<DayNightCycle>();
        _timeRemaining = _totalTime;
        _isGameOver = false;
    }

    public void ResetTimer()
    {
        _timeRemaining = _totalTime;
        _isGameOver = false;
        UpdateHourglass(_totalTime);
    }

    public void StartTimerAndCycle()
    {
        ResetTimer();
        StartCoroutine(StartCycleIEnumerator());
    }

    private void TriggerGameOver()
    {
        OnGameLost?.Invoke();
    }

    private void UpdateHourglass(float currentTime)
    {
        if (_hourglassImage == null)
            return;

        _hourglassImage.fillAmount = Mathf.InverseLerp(0f, _totalTime, currentTime);
    }

    private IEnumerator StartCycleIEnumerator()
    {
        while (true)
        {
            if (_isGameOver)
                yield break;

            _timeRemaining -= Time.deltaTime;

            if (_timeRemaining <= 0f)
            {
                _timeRemaining = 0f;
                _isGameOver = true;
                TriggerGameOver();
            }

            UpdateHourglass(_timeRemaining);
            _dayNightCycle.ApplyCycle(1f - (_timeRemaining / _totalTime));
            yield return null;
        }
    }
}
