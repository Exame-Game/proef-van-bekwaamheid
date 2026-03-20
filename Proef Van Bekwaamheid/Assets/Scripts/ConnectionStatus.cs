using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ConnectionStatus : NetworkBehaviour
{
    [SerializeField] private GameObject _connectingScreen;

    private GameObject _currentPopUp;

    private InputAction _connectAction;
    private InputAction _disconnectAction;

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        InitializeInputActions();
    }

    private void Update()
    {
        if (_connectAction.IsPressed())
            OnConnectButtonClicked();

        if (_disconnectAction.IsPressed())
            OnDisconnectButtonClicked();
    }

    private void InitializeInputActions()
    {
        _connectAction = new InputAction(binding: "<Keyboard>/c");
        _disconnectAction = new InputAction(binding: "<Keyboard>/d");

        _connectAction.Enable();
        _disconnectAction.Enable();
    }

    public void OnConnectButtonClicked()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Already connected!");
            return;
        }

        InitializeConnectingScreen();

        NetworkManager.Singleton.StartClient();
    }

    private void OnDisconnectButtonClicked()
    {
        Debug.Log("Disconnecting...");
    }

    private void OnClientConnected(ulong clientId)
    {
        Destroy(_currentPopUp);
        Debug.Log("Connected! Client ID: " + clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log("Disconnected! Client ID: " + clientId);
    }

    private void InitializeConnectingScreen()
    {
        GameObject connectingScreen = Instantiate(_connectingScreen);
        _currentPopUp = connectingScreen;
    }

    private void OnDestroy()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }
}

