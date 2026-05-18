using System.Net;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.Multiplayer.Center.Common.Analytics;



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

    private bool _isHost;

    private void OnEnable()
    {
        StartCoroutine(WaitForNetworkManager());
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void Start()
    {
#if UNITY_EDITOR

        IReadOnlyList<string> tags = CurrentPlayer.Tags;
        if (tags != null)
            foreach (string tag in tags)
                if (tag == "Host")
                {
                    _isHost = true;
                    break;
                }

        if (_isHost)
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

    public void StartHost()
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.UTF8.GetBytes("host");
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
        _isScanning = false;
        if (_scanCoroutine != null)
            StopCoroutine(_scanCoroutine);

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, 7777);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.UTF8.GetBytes("client");

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
        string role = System.Text.Encoding.UTF8.GetString(request.Payload);

        bool isApproved = role != "host";

        response.Approved = isApproved;

        if (!isApproved)
            return;

        response.CreatePlayerObject = true;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost)
            return;

        UIManager.Instance.SetClientUIState(ClientUIState.QRScanner);

        if (_scanCoroutine != null)
            StopCoroutine(_scanCoroutine);

        _scanCoroutine = StartCoroutine(ScanQRLoop());
    }

    private IEnumerator ScanQRLoop()
    {
        _isScanning = true;
        WaitForSeconds wait = new WaitForSeconds(QRCodeScanner.scanInterval);

        yield return null; // extra frame

        while (_isScanning && !NetworkManager.Singleton.IsHost)
        {
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                _isScanning = false;
                yield break;
            }

            QRCodeScanner.Scan();
            yield return wait;
        }
    }

    private IEnumerator WaitForNetworkManager()
    {
        yield return new WaitUntil(() => NetworkManager.Singleton != null);
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }
}
