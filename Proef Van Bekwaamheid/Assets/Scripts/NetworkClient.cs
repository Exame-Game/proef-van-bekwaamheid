using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class NetworkClient : MonoBehaviour
{
    [SerializeField] private UnityTransport transport;
    [SerializeField] private TMP_InputField ipInput;

    public System.Action<bool> OnConnectionChanged;

    public void StartClient()
    {
        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("Already connected — disconnect first.");
            return;
        }

        if (string.IsNullOrEmpty(ipInput.text))
        {
            Debug.LogError("IP field is empty!");
            return;
        }

        string ipAddress = ipInput.text.Trim();
        SetIpAddress(ipAddress);

        bool started = NetworkManager.Singleton.StartClient();

        if (!started)
        {
            Debug.LogError("Failed to start client.");
            return;
        }

        Debug.Log($"Connecting to: {ipAddress}");
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientFailedToConnect;
    }

    public void StopClient()
    {
        NetworkManager.Singleton.Shutdown();
        ResetAddress();
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
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
            return;

        OnConnectionChanged?.Invoke(true);
        UnsubscribeCallbacks();
    }

    private void OnClientFailedToConnect(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
            return;

        Debug.LogWarning("Failed to connect to host.");
        NetworkManager.Singleton.Shutdown();
        ResetAddress();
        OnConnectionChanged?.Invoke(false);
        UnsubscribeCallbacks();
    }

    private void UnsubscribeCallbacks()
    {
        if (NetworkManager.Singleton == null) 
            return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientFailedToConnect;
    }

    private void OnDestroy()
    {
        UnsubscribeCallbacks();
    }
}
