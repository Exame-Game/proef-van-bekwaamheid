using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Initial Client State")]
    [SerializeField] private ClientUIState initialState = ClientUIState.QRScanner;

    [Header("Initial Host State")]
    [SerializeField] private HostUIState initialHostState = HostUIState.Lobby;

    [Header("Client UI Panels")]
    [SerializeField] private GameObject ClientMenuUI;
    [SerializeField] private GameObject ClientQRScannerUI;
    [SerializeField] private GameObject ClientManualConnectionUI;
    [SerializeField] private GameObject ClientDisconnectUI;
    [SerializeField] private GameObject ClientControllerUI;
    [SerializeField] private GameObject ClientSettingsUI;
    [SerializeField] private GameObject ClientControlSettingsUI;

    [Header("Host UI Panels")]
    [SerializeField] private GameObject HostLobbyUI;
    [SerializeField] private GameObject HostGameUI;

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetClientUIState(initialState);
        SetHostUIState(initialHostState);
    }

    private void SetHostUIState(HostUIState initialHostState)
    {
        HostGameUI.SetActive(initialHostState == HostUIState.Game);
        HostLobbyUI.SetActive(initialHostState == HostUIState.Lobby);
    }

    public void SetClientUIState(ClientUIState state)
    {
        ClientMenuUI.SetActive(state == ClientUIState.Menu);
        ClientQRScannerUI.SetActive(state == ClientUIState.QRScanner);
        ClientManualConnectionUI.SetActive(state == ClientUIState.ManualConnection);
        ClientDisconnectUI.SetActive(state == ClientUIState.Disconnect);
        ClientControllerUI.SetActive(state == ClientUIState.Controller);
        ClientSettingsUI.SetActive(state == ClientUIState.Settings);
        ClientControlSettingsUI.SetActive(state == ClientUIState.ControlSettings);
    }

    public void SetClientUIStateFromInt(int stateIndex)
    {
        SetClientUIState((ClientUIState)stateIndex);
    }

    public void SetHostUIStateFromInt(int stateIndex)
    {
        SetHostUIState((HostUIState)stateIndex);
    }
}

public enum ClientUIState
{
    Menu = 0,
    QRScanner = 1,
    ManualConnection = 2,
    Disconnect = 3,
    Controller = 4,
    Settings = 5,
    ControlSettings = 6
}

public enum HostUIState
{
    Lobby,
    Game
}
