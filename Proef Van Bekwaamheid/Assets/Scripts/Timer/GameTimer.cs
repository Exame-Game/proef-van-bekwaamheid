using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class GameTimer : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float totalTime = 20f;

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
        if (!IsServer || gameOver.Value)
            return;

        timeRemaining.Value -= Time.deltaTime;

        if (timeRemaining.Value > 0f)
            return;

        timeRemaining.Value = 0f;
        gameOver.Value = true;
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

        float amount = Map(currentTime, 0f, totalTime, 0f, 1f);
        Debug.Log($"Updating hourglass: {currentTime} seconds remaining, fill amount: {amount}");

        hourglassImage.fillAmount = amount;
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

    float Map(float value, float inMin, float inMax, float outMin, float outMax)
    {
        return outMin + (Mathf.Clamp01((value - inMin) / (inMax - inMin)) * (outMax - outMin));
    }
}
