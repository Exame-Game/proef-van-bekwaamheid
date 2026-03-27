using System.Net;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System;
using System.Collections;



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

        if (Array.IndexOf(tags, "Host") >= 0)
        {
            hostUI.SetActive(true);
            clientUI.SetActive(false);
            StartHost();
        }
        else
        {
            hostUI.SetActive(false);
            clientUI.SetActive(true);
            Debug.Log("<color=cyan>[ConnectionManager] Editor play mode detected — initializing as client and starting QR code scan loop...</color>");
            QRCodeScanner.OnIPDecoded += StartClient;
            StartCoroutine(ScanQRLoop());
        }
#elif HOST_BUILD
        hostUI.SetActive(true);
        clientUI.SetActive(false);
        StartHost();
#else
        hostUI.SetActive(false);
        clientUI.SetActive(true);
        QRCodeScanner.OnIPDecoded += StartClient;
        StartCoroutine(ScanQRLoop());
#endif
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        QRCodeGenerator.GenerateQRCode(GetLocalIPAddress());
    }

    public void StartClient(string ip)
    {
        Debug.Log($"[ConnectionManager] Starting client with IP: {ip}");
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, 7777);
        NetworkManager.Singleton.StartClient();
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

    private IEnumerator ScanQRLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(QRCodeScanner.scanInterval);
        while (!NetworkManager.Singleton.IsConnectedClient)
        {
            Debug.Log(NetworkManager.Singleton.IsConnectedClient);
            QRCodeScanner.Scan();
            yield return wait;
        }

        Debug.Log("[ConnectionManager] Connection established, stopping scan loop.");
    }
}