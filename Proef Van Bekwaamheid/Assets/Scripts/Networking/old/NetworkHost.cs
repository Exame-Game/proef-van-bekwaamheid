using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Net;
using System.Net.Sockets;

public class NetworkHost : NetworkBehaviour
{
    [SerializeField] private UnityTransport transport;
    [SerializeField] private TextMeshProUGUI ipAddressText;

    public System.Action<bool> OnConnectionChanged;

    public string CurrentIPAddress { get; set; }

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            Debug.Log("<color=cyan>[NetworkHost] Already server/host — skipping StartHost().</color>");
            return;
        }

        if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("<color=cyan>[NetworkHost] Running as pure client — skipping StartHost().</color>");
            return;
        }

        StartHost();
    }

    public void StartHost()
    {
        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("<color=orange>[NetworkHost] StartHost() called but already listening — shut down first.</color>");
            return;
        }

        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = "0.0.0.0";
        Debug.Log("<color=cyan>[NetworkHost] Transport address set to 0.0.0.0 — starting host...</color>");

        bool started = NetworkManager.Singleton.StartHost();

        if (started)
        {
            string ip = GetLocalIPAddress();
            Debug.Log($"<color=green>[NetworkHost] Host started successfully! Listening on IP: {ip}</color>");
            OnConnectionChanged?.Invoke(true);
        }
        else
        {
            Debug.LogError("<color=red>[NetworkHost] Failed to start host. Port may already be in use.</color>");
        }
    }

    public void StopHost()
    {
        Debug.Log("<color=orange>[NetworkHost] StopHost() called — shutting down NetworkManager...</color>");
        NetworkManager.Singleton.Shutdown();
        ResetAddress();

        if (ipAddressText != null)
            ipAddressText.text = string.Empty;

        Debug.Log("<color=orange>[NetworkHost] Host stopped. Address reset.</color>");
        OnConnectionChanged?.Invoke(false);
    }

    public string GetLocalIPAddress()
    {
        Debug.Log("<color=cyan>[NetworkHost] Resolving local IPv4 address...</color>");

        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily != AddressFamily.InterNetwork)
                continue;

            CurrentIPAddress = ip.ToString();
            ipAddressText.text = CurrentIPAddress;
            Debug.Log($"<color=green>[NetworkHost] Local IP resolved: {CurrentIPAddress}</color>");
            return CurrentIPAddress;
        }

        Debug.LogError("<color=red>[NetworkHost] GetLocalIPAddress() failed — no IPv4 adapter found!</color>");
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    private void ResetAddress()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = "0.0.0.0";
        Debug.Log("<color=cyan>[NetworkHost] Transport address reset to 0.0.0.0</color>");
    }
}