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
        switch (state)
        {
            case ClientUIState.QRScanner:
                QRScannerUI.SetActive(true);
                ManualConnectionUI.SetActive(false);
                DisconnectUI.SetActive(false);
                ControllerUI.SetActive(false);
                break;
            case ClientUIState.ManualConnection:
                QRScannerUI.SetActive(false);
                ManualConnectionUI.SetActive(true);
                DisconnectUI.SetActive(false);
                ControllerUI.SetActive(false);
                break;
            case ClientUIState.Disconnect:
                QRScannerUI.SetActive(false);
                ManualConnectionUI.SetActive(false);
                DisconnectUI.SetActive(true);
                ControllerUI.SetActive(false);
                break;
            case ClientUIState.Controller:
                QRScannerUI.SetActive(false);
                ManualConnectionUI.SetActive(false);
                DisconnectUI.SetActive(false);
                ControllerUI.SetActive(true);
                break;
        }
    }
}

public enum ClientUIState
{
    QRScanner,
    ManualConnection,
    Disconnect,
    Controller
}
