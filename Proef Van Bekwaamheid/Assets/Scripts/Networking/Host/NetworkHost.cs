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

    public void StartHost()
    {
        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("Already listening — shut down first.");
            return;
        }

        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = "0.0.0.0";

        bool started = NetworkManager.Singleton.StartHost();

        if (started)
        {
            GetLocalIPAddress();
            OnConnectionChanged?.Invoke(true);
        }
        else
            Debug.LogError("Failed to start host. Port may already be in use.");
    }

    public void StopHost()
    {
        NetworkManager.Singleton.Shutdown();
        ResetAddress();

        if (ipAddressText != null) 
            ipAddressText.text = string.Empty;

        OnConnectionChanged?.Invoke(false);
    }

    public string GetLocalIPAddress()
    {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                ipAddressText.text = ip.ToString();
                return ip.ToString();
            }

        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    private void ResetAddress()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = "0.0.0.0";
    }
}
