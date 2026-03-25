using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class NetworkClient : NetworkBehaviour
{
    [SerializeField] private UnityTransport transport;
    [SerializeField] private TMP_InputField ipInput;
    [SerializeField] private ConnectionStatus connectionStatus;

    public System.Action<bool> OnConnectionChanged;

    public void StartClient()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("<color=orange>[NetworkClient] StartClient() blocked — already a client or running as host.</color>");
            return;
        }

        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("<color=orange>[NetworkClient] StartClient() blocked — already listening. Disconnect first.</color>");
            return;
        }

        if (string.IsNullOrEmpty(ipInput.text))
        {
            Debug.LogError("<color=red>[NetworkClient] StartClient() blocked — IP input field is empty!</color>");
            return;
        }

        string ipAddress = ipInput.text.Trim();
        Debug.Log($"<color=cyan>[NetworkClient] Attempting to connect to: {ipAddress}...</color>");

        SetIpAddress(ipAddress);
        Debug.Log($"<color=cyan>[NetworkClient] Transport address set to: {ipAddress}</color>");

        NetworkManager.Singleton.NetworkConfig.EnableSceneManagement = false;

        bool started = NetworkManager.Singleton.StartClient();

        if (!started)
        {
            Debug.LogError("<color=red>[NetworkClient] StartClient() failed — NetworkManager could not start client.</color>");
            return;
        }

        Debug.Log($"<color=cyan>[NetworkClient] Client started — waiting for connection to {ipAddress}...</color>");

        //connectionStatus.OnConnectButtonClicked();
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientFailedToConnect;
        Debug.Log("<color=cyan>[NetworkClient] Subscribed to connected/disconnect callbacks.</color>");
    }

    public void StopClient()
    {
        if (!NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("<color=orange>[NetworkClient] StopClient() blocked — not running as a pure client.</color>");
            return;
        }

        Debug.Log("<color=orange>[NetworkClient] StopClient() called — shutting down...</color>");
        NetworkManager.Singleton.Shutdown();
        ResetAddress();
        Debug.Log("<color=orange>[NetworkClient] Shutdown complete. Address reset.</color>");
        OnConnectionChanged?.Invoke(false);
    }

    private void SetIpAddress(string ipAddress)
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = ipAddress;
    }

    private void ResetAddress()
    {
        SetIpAddress("0.0.0.0");
        Debug.Log("<color=cyan>[NetworkClient] Transport address reset to 0.0.0.0</color>");
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log($"<color=cyan>[NetworkClient] Another client connected (ID: {clientId}) — not us, ignoring.</color>");
            return;
        }

        Debug.Log($"<color=green>[NetworkClient] Successfully connected! Local client ID: {clientId}</color>");
        OnConnectionChanged?.Invoke(true);
        UnsubscribeCallbacks();
    }

    private void OnClientFailedToConnect(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log($"<color=cyan>[NetworkClient] A different client disconnected (ID: {clientId}) — not us, ignoring.</color>");
            return;
        }

        Debug.LogWarning($"<color=red>[NetworkClient] Failed to connect or lost connection! Client ID: {clientId}</color>");
        NetworkManager.Singleton.Shutdown();
        ResetAddress();
        OnConnectionChanged?.Invoke(false);
        UnsubscribeCallbacks();
    }

    private void UnsubscribeCallbacks()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("<color=orange>[NetworkClient] UnsubscribeCallbacks() — NetworkManager.Singleton is null, skipping.</color>");
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientFailedToConnect;
        Debug.Log("<color=orange>[NetworkClient] Unsubscribed from NetworkManager callbacks.</color>");
    }

    private void OnDestroy()
    {
        Debug.Log("<color=orange>[NetworkClient] OnDestroy — cleaning up callbacks.</color>");
        UnsubscribeCallbacks();
    }
}