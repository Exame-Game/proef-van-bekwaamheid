using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using DG.Tweening;

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
    [SerializeField] private GameObject ClientReadyUI;

    [Header("Host UI Panels")]
    [SerializeField] private GameObject HostLobbyUI;
    [SerializeField] private GameObject HostGameUI;

    [Header("Slide Settings")]
    [SerializeField] private float slideDuration = 0.35f;
    [SerializeField] private Ease slideEase = Ease.OutCubic;
    [SerializeField] private Vector2 hiddenOffset = new Vector2(-4000f, 0f);

    public static UIManager Instance { get; private set; }

    private GameObject _activeClientPanel;
    private GameObject _activeHostPanel;

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
        HideAllInstant();
        SetClientUIState(initialState);
        SetHostUIState(initialHostState);
    }

    private void HideAllInstant()
    {
        GameObject[] all = {
            ClientMenuUI, ClientQRScannerUI, ClientManualConnectionUI,
            ClientDisconnectUI, ClientControllerUI, ClientSettingsUI,
            ClientControlSettingsUI, ClientReadyUI, HostLobbyUI, HostGameUI
        };

        foreach (GameObject panel in all)
        {
            if (panel == null) 
                continue;

            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchoredPosition = hiddenOffset;
            panel.SetActive(false);
        }
    }

    private void SlideIn(GameObject panel)
    {
        if (panel == null) 
            return;

        panel.SetActive(true);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchoredPosition = hiddenOffset;
        rect.DOAnchorPos(Vector2.zero, slideDuration)
            .SetEase(slideEase)
            .SetUpdate(true);
    }

    private void SlideOut(GameObject panel, Action onComplete = null)
    {
        if (panel == null)
        {
            onComplete?.Invoke();
            return;
        }
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.DOAnchorPos(hiddenOffset, slideDuration)
            .SetEase(slideEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                panel.SetActive(false);
                onComplete?.Invoke();
            });
    }

    public void SetClientUIState(ClientUIState state)
    {
        GameObject next = state switch
        {
            ClientUIState.Menu => ClientMenuUI,
            ClientUIState.QRScanner => ClientQRScannerUI,
            ClientUIState.ManualConnection => ClientManualConnectionUI,
            ClientUIState.Disconnect => ClientDisconnectUI,
            ClientUIState.Controller => ClientControllerUI,
            ClientUIState.Settings => ClientSettingsUI,
            ClientUIState.ControlSettings => ClientControlSettingsUI,
            ClientUIState.Ready => ClientReadyUI,
            _ => null
        };

        if (next == _activeClientPanel) 
            return;

        GameObject previous = _activeClientPanel;
        _activeClientPanel = next;
        SlideOut(previous, () => SlideIn(next));
    }

    private void SetHostUIState(HostUIState state)
    {
        GameObject next = state switch
        {
            HostUIState.Lobby => HostLobbyUI,
            HostUIState.Game => HostGameUI,
            _ => null
        };

        if (next == _activeHostPanel) 
            return;

        GameObject previous = _activeHostPanel;
        _activeHostPanel = next;
        SlideOut(previous, () => SlideIn(next));
    }

    public void SetClientUIStateFromInt(int stateIndex) => SetClientUIState((ClientUIState)stateIndex);
    public void SetHostUIStateFromInt(int stateIndex) => SetHostUIState((HostUIState)stateIndex);
}

public enum ClientUIState
{
    Menu = 0,
    QRScanner = 1,
    ManualConnection = 2,
    Disconnect = 3,
    Controller = 4,
    Settings = 5,
    ControlSettings = 6, 
    Ready = 7
}

public enum HostUIState
{
    Lobby,
    Game
}