using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Net;
using System.Net.Sockets;

public class NetworkUI : MonoBehaviour
{
    [Header("Network")]
    [SerializeField] private UnityTransport transport;
    [SerializeField] private TextMeshProUGUI ipAddressText;
    [SerializeField] private TMP_InputField ip;
    [SerializeField] private string ipAddress;

    [Header("UI Panels")]
    [SerializeField] private GameObject connectPanel;       // ConnectNetworkUIElement
    [SerializeField] private GameObject disconnectPanel;    // DisconnectNetworkUIElement
    [SerializeField] private GameObject controllerPanel;    // ControllerUIElement

    [Header("References")]
    [SerializeField] private ToggleGameObject toggler;      // ToggleGameObject script reference

    private PlayerMovement _movement;
    private bool _pcAssigned;

    void Start()
    {
        Initialize();
    }

    public void ToggleSettings()
    {
        bool isConnected = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

        if (isConnected)
        {
            if (connectPanel.activeSelf)
                toggler.SlideOut(connectPanel, connectPanel.GetComponent<RectTransform>());

            toggler.ToggleActive(disconnectPanel);
        }
        else
        {
            if (disconnectPanel.activeSelf)
                toggler.SlideOut(disconnectPanel, disconnectPanel.GetComponent<RectTransform>());

            toggler.ToggleActive(connectPanel);
        }

        toggler.ToggleActive(controllerPanel);
    }

    public void StartHost()
    {
        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("Already listening — shut down first.");
            return;
        }

        ipAddress = "0.0.0.0";
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = ipAddress;

        bool started = NetworkManager.Singleton.StartHost();

        if (started)
        {
            GetLocalIPAddress();
            SetConnectedUI(true);
        }
        else
        {
            Debug.LogError("Failed to start host. Port may already be in use.");
        }
    }

    public void StopHost()
    {
        NetworkManager.Singleton.Shutdown();
        ResetAfterDisconnect();
    }

    public void StartClient()
    {
        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("Already connected — disconnect first.");
            return;
        }

        if (string.IsNullOrEmpty(ip.text))
        {
            Debug.LogError("IP field is empty!");
            return;
        }

        ipAddress = ip.text.Trim();
        SetIpAddress();

        bool started = NetworkManager.Singleton.StartClient();

        if (started)
        {
            Debug.Log($"Connecting to: {ipAddress}");
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientFailedToConnect;
        }
        else
        {
            Debug.LogError("Failed to start client.");
        }
    }

    public void StopClient()
    {
        NetworkManager.Singleton.Shutdown();
        ResetAfterDisconnect();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            SetConnectedUI(true);
            UnsubscribeNetworkCallbacks();
        }
    }

    private void OnClientFailedToConnect(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.LogWarning("Failed to connect to host.");
            NetworkManager.Singleton.Shutdown();
            ResetAfterDisconnect();
            UnsubscribeNetworkCallbacks();
        }
    }

    private void UnsubscribeNetworkCallbacks()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientFailedToConnect;
    }

    private void SetConnectedUI(bool isConnected)
    {
        toggler.HideInstant(connectPanel);
        toggler.HideInstant(disconnectPanel);
        toggler.HideInstant(controllerPanel);

        if (isConnected)
            toggler.SlideIn(disconnectPanel, disconnectPanel.GetComponent<RectTransform>());
        else
            toggler.SlideIn(connectPanel, connectPanel.GetComponent<RectTransform>());
    }

    private void ResetAfterDisconnect()
    {
        ipAddress = "0.0.0.0";
        SetIpAddress();

        if (ipAddressText != null)
            ipAddressText.text = string.Empty;

        _pcAssigned = false;
        _movement = null;

        SetConnectedUI(false);

        InvokeRepeating(nameof(assignPlayerController), 0.1f, 0.1f);
    }

    public string GetLocalIPAddress()
    {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                ipAddressText.text = ip.ToString();
                ipAddress = ip.ToString();
                return ip.ToString();
            }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    public void SetIpAddress()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = ipAddress;
    }

    public void Right() { if (_pcAssigned) _movement.Movement("Right"); }
    public void Left() { if (_pcAssigned) _movement.Movement("Left"); }
    public void Forward() { if (_pcAssigned) _movement.Movement("Forward"); }
    public void Back() { if (_pcAssigned) _movement.Movement("Back"); }

    private void Initialize()
    {
        ipAddress = "0.0.0.0";
        SetIpAddress();
        _pcAssigned = false;

        toggler.HideInstant(disconnectPanel);
        toggler.HideInstant(controllerPanel);
        connectPanel.SetActive(true);

        InvokeRepeating(nameof(assignPlayerController), 0.1f, 0.1f);
    }

    private void assignPlayerController()
    {
        if (_movement == null)
            _movement = FindFirstObjectByType<PlayerMovement>();
        else if (_movement == FindFirstObjectByType<PlayerMovement>())
        {
            _pcAssigned = true;
            CancelInvoke();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeNetworkCallbacks();
    }
}