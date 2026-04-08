using System.Net;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using TMPro;



#if UNITY_EDITOR
using Unity.Multiplayer.PlayMode;
#endif

public class ConnectionManager : MonoBehaviour
{
    [SerializeField] private GameObject hostUI;
    [SerializeField] private GameObject clientUI;
    [SerializeField] private QRCodeGenerator QRCodeGenerator;
    [SerializeField] private QRCodeScanner QRCodeScanner;

    [SerializeField] private TMP_InputField clientIpInputField;

    private Coroutine _scanCoroutine;
    private bool _isScanning;

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

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
            UIManager.Instance.SetClientUIState(ClientUIState.QRScanner);
            Debug.Log("<color=cyan>[ConnectionManager] Editor play mode detected — initializing as client and starting QR code scan loop...</color>");
            QRCodeScanner.OnIPDecoded += StartClient;
            _scanCoroutine = StartCoroutine(ScanQRLoop());
        }
#elif HOST_BUILD
        hostUI.SetActive(true);
        clientUI.SetActive(false);
        StartHost();
#else
        hostUI.SetActive(false);
        clientUI.SetActive(true);
        QRCodeScanner.OnIPDecoded += StartClient;
        _scanCoroutine = StartCoroutine(ScanQRLoop());
#endif
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost)
            return;

        Debug.Log("[ConnectionManager] Client disconnected — restarting QR scan loop.");
        UIManager.Instance.SetClientUIState(ClientUIState.QRScanner);

        if (_scanCoroutine != null)
            StopCoroutine(_scanCoroutine);

        _scanCoroutine = StartCoroutine(ScanQRLoop());
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        NetworkManager.Singleton.StartHost();
        QRCodeGenerator.GenerateQRCode(GetLocalIPAddress());
    }

    public void StopHost()
    {
        NetworkManager.Singleton.Shutdown();
    }

    public void StartClient(string ip)
    {
        _isScanning = false; // stop loop before connecting
        if (_scanCoroutine != null)
            StopCoroutine(_scanCoroutine);

        Debug.Log($"[ConnectionManager] Starting client with IP: {ip}");
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, 7777);
        NetworkManager.Singleton.StartClient();
    }

    public void StartClient()
    {
        string ip = clientIpInputField.text;

        StartClient(ip);
    }

    public void StopClient()
    {
        if (!NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
            return;

        NetworkManager.Singleton.Shutdown();
    }

    public void StartQRLoop()
    {
        StartCoroutine(ScanQRLoop());
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

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
        response.CreatePlayerObject = false;
    }

    private IEnumerator ScanQRLoop()
    {
        _isScanning = true;
        WaitForSeconds wait = new WaitForSeconds(QRCodeScanner.scanInterval);

        // Wait a frame so Netcode finishes cleaning up IsConnectedClient state
        yield return null;

        while (_isScanning && !NetworkManager.Singleton.IsHost)
        {
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                Debug.Log("[ConnectionManager] Connection established, stopping scan loop.");
                _isScanning = false;
                yield break;
            }

            QRCodeScanner.Scan();
            yield return wait;
        }
    }
}