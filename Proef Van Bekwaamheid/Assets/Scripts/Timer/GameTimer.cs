using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
    private const float k_DefaultTime = 20f;

    [SerializeField] private Image _hourglassImage;

    public float _totalTime = k_DefaultTime;

    private float _timeRemaining;
    private bool _isGameOver;

    private void Start()
    {
        _timeRemaining = _totalTime;
    }

    private void Update()
    {
        if (_isGameOver)
            return;

        _timeRemaining -= Time.deltaTime;

        if (_timeRemaining <= 0f)
        {
            _timeRemaining = 0f;
            _isGameOver = true;
            TriggerGameOver();
        }

        UpdateHourglass(_timeRemaining);
    }

    public void ResetTimer()
    {
        _timeRemaining = _totalTime;
        _isGameOver = false;
    }

    private void TriggerGameOver()
    {
        Debug.Log("Time's up! You lose.");
    }

    private void UpdateHourglass(float currentTime)
    {
        if (_hourglassImage == null)
            return;

        _hourglassImage.fillAmount = Mathf.InverseLerp(0f, _totalTime, currentTime);
    }
}
