using System.Net;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System;


#if UNITY_EDITOR
using Unity.Multiplayer.PlayMode;
#endif

public class ConnectionManager : MonoBehaviour
{
    [SerializeField] private GameObject hostUI;
    [SerializeField] private GameObject clientUI;

    [SerializeField] private QRCodeGenerator QRCodeGenerator;
    [SerializeField] private QRCodeScanner QRCodeScanner;

    private void Start()
    {
#if UNITY_EDITOR
        string[] tags = CurrentPlayer.ReadOnlyTags();

        if (System.Array.IndexOf(tags, "Host") >= 0)
        {
            hostUI.SetActive(true);
            clientUI.SetActive(false);
            StartHost();
        }
        else
        {
            hostUI.SetActive(false);
            clientUI.SetActive(true);
            QRCodeScanner.OnIPDecoded += StartClient;
        }
#elif HOST_BUILD
        hostUI.SetActive(true);
        clientUI.SetActive(false);
        StartHost();
#else
        hostUI.SetActive(false);
        clientUI.SetActive(true);
        QRCodeScanner.OnIPDecoded += StartClient;
#endif
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        QRCodeGenerator.GenerateQRCode(GetLocalIPAddress());
    }

    public void StartClient(string ip)
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, 7777);
        NetworkManager.Singleton.StartClient();

        while (!NetworkManager.Singleton.IsConnectedClient)
        {
            Debug.Log("<color=yellow>[ConnectionManager] Attempting to connect to host...</color>");
            QRCodeScanner.Scan();
        }
    }

    private string GetLocalIPAddress()
    {
        string ip = "";
        foreach (IPAddress addr in Dns.GetHostAddresses(Dns.GetHostName()))
            if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                ip = addr.ToString();
                break;
            }

        return ip;
    }
}