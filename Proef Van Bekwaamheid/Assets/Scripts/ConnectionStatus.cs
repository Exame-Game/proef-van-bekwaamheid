using Unity.Netcode;
using UnityEngine;

public class ConnectionStatus : MonoBehaviour
{
    [SerializeField] private GameObject _connectingScreen;

    private GameObject _currentPopUp;

    private void Start()
    {
        Debug.Log("<color=cyan>[ConnectionStatus] Initializing — subscribing to NetworkManager callbacks...</color>");
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    public void OnConnectButtonClicked()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("<color=orange>[ConnectionStatus] Already connected, ignoring.</color>");
            return;
        }

        Debug.Log("<color=cyan>[ConnectionStatus] Connect triggered — showing connecting screen.</color>");
        InitializeConnectingScreen();
        // Note: actual StartClient() is handled by NetworkClient.cs
    }

    private void OnClientConnected(ulong clientId)
    {
        if (_currentPopUp != null)
        {
            Destroy(_currentPopUp);
            _currentPopUp = null;
        }

        bool isLocal = clientId == NetworkManager.Singleton.LocalClientId;
        Debug.Log($"<color=green>[ConnectionStatus] Client connected! ID: {clientId}{(isLocal ? " (us)" : "")}</color>");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        // Show connecting screen again if we got dropped
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("<color=red>[ConnectionStatus] We disconnected — hiding connecting screen.</color>");
            if (_currentPopUp != null)
            {
                Destroy(_currentPopUp);
                _currentPopUp = null;
            }
        }
    }

    private void InitializeConnectingScreen()
    {
        if (_connectingScreen == null)
        {
            Debug.LogError("<color=red>[ConnectionStatus] _connectingScreen prefab is null!</color>");
            return;
        }

        if (_currentPopUp != null)
            Destroy(_currentPopUp);

        _currentPopUp = Instantiate(_connectingScreen);
        Debug.Log("<color=cyan>[ConnectionStatus] Connecting screen instantiated.</color>");
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
}