using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerReadyManager : MonoBehaviour
{
    [SerializeField] private Button readyButton;
    [SerializeField] private Text readyStatusText;

    private bool isPlayerReady = false;
    private ReadySystem readySystem;

    private void Start()
    {
        readySystem = ReadySystem.Instance;

        if (readySystem != null)
        {
            readySystem.AllPlayersReadyEvent += OnAllPlayersReady;
        }

        // Setup button if assigned
        if (readyButton != null)
        {
            readyButton.onClick.AddListener(ToggleReady);
        }

        UpdateReadyUI();
    }

    private void OnDestroy()
    {
        if (readySystem != null)
        {
            readySystem.AllPlayersReadyEvent -= OnAllPlayersReady;
        }
    }

    private void Update()
    {
    }

    public void ToggleReady()
    {
        // Only allow if you're a client
        if (!NetworkManager.Singleton.IsConnectedClient)
        {
            Debug.LogWarning("Not connected to server!");
            return;
        }

        isPlayerReady = !isPlayerReady;

        // Send ready status to server
        SendReadyStatusToServer(isPlayerReady);

        UpdateReadyUI();
    }

    private void SendReadyStatusToServer(bool ready)
    {
        // Call the RPC on the server
        readySystem.PlayerReadyRpc(ready);

        Debug.Log(ready ? "You are ready!" : "You are not ready.");
    }

    private void UpdateReadyUI()
    {
        if (readyButton != null)
        {
            // Change button color based on ready status
            ColorBlock colors = readyButton.colors;
            colors.normalColor = isPlayerReady ? Color.green : Color.red;
            readyButton.colors = colors;
        }

        if (readyStatusText != null)
        {
            readyStatusText.text = isPlayerReady ? "READY" : "NOT READY";
            readyStatusText.color = isPlayerReady ? Color.green : Color.red;
        }
    }

    private void OnAllPlayersReady()
    {
        Debug.Log("Game is starting!");
        // Add any client-side logic when game starts
    }

    public bool IsReady()
    {
        return isPlayerReady;
    }
}