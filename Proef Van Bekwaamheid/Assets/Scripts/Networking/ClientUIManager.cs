using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ClientUIManager : MonoBehaviour
{
    [SerializeField] private ClientUIState initialState = ClientUIState.QRScanner;

    [SerializeField] private GameObject QRScannerUI;
    [SerializeField] private GameObject ManualConnectionUI;
    [SerializeField] private GameObject DisconnectUI;

    [SerializeField] private GameObject ControllerUI;

    public static ClientUIManager Instance { get; private set; }

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
        SetUIState(initialState);
    }

    public void SetUIState(ClientUIState state)
    {
        QRScannerUI.SetActive(state == ClientUIState.QRScanner);
        ManualConnectionUI.SetActive(state == ClientUIState.ManualConnection);
        DisconnectUI.SetActive(state == ClientUIState.Disconnect);
        ControllerUI.SetActive(state == ClientUIState.Controller);
    }

    public void SetUIStateFromInt(int stateIndex)
    {
        SetUIState((ClientUIState)stateIndex);
    }
}

public enum ClientUIState
{
    QRScanner,
    ManualConnection,
    Disconnect,
    Controller
}
