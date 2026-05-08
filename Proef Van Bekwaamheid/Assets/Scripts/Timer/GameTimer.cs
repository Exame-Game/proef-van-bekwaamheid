using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class GameTimer : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float totalTime = 180f;

    [Header("UI References")]
    [SerializeField] private Image hourglassImage;

    private NetworkVariable<float> timeRemaining = new NetworkVariable<float>(
        20f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<bool> gameOver = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        timeRemaining.OnValueChanged += OnTimeChanged;
        gameOver.OnValueChanged += OnGameOverChanged;

        UpdateHourglass(timeRemaining.Value);
    }

    public override void OnNetworkDespawn()
    {
        timeRemaining.OnValueChanged -= OnTimeChanged;
        gameOver.OnValueChanged -= OnGameOverChanged;
    }

    private void Update()
    {
        if (!IsServer) 
            return;

        if (gameOver.Value) 
            return;

        timeRemaining.Value -= Time.deltaTime;

        if (timeRemaining.Value <= 0f)
        {
            timeRemaining.Value = 0f;
            gameOver.Value = true;
        }
    }

    private void OnTimeChanged(float previous, float current)
    {
        UpdateHourglass(current);
    }

    private void OnGameOverChanged(bool previous, bool current)
    {
        if (current)
            TriggerGameOver();
    }

    private void UpdateHourglass(float currentTime)
    {
        if (hourglassImage == null) 
            return;

        Debug.Log($"Updating hourglass: {currentTime:F2} seconds remaining");
        hourglassImage.fillAmount = Map(timeRemaining.Value, 0f, 1);
        Debug.Log($"Hourglass fill amount: {hourglassImage.fillAmount:F2}");
    }

    private void TriggerGameOver()
    {
        Debug.Log("Time's up! You lose.");
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetTimerServerRpc()
    {
        timeRemaining.Value = totalTime;
        gameOver.Value = false;
    }

    float Map(
        float value, 
        float inMin, 
        float inMax)
    {
        return Mathf.Clamp01((value - inMin) / (inMax - inMin));
    }
}