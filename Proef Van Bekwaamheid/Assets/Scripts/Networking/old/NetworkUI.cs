using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Manages panel visibility based on connection state and role (host vs client).
/// Host sees: connect/disconnect panels only.
/// Client sees: connect/disconnect panels + controller panel.
/// </summary>
public class NetworkUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject connectPanel;
    [SerializeField] private GameObject disconnectPanel;
    [SerializeField] private GameObject controllerPanel;

    [Header("References")]
    [SerializeField] private ToggleGameObject toggler;
    [SerializeField] private NetworkHost networkHost;
    [SerializeField] private NetworkClient networkClient;

    private bool IsHost => NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
    private bool IsClient => NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost;

    private void Start()
    {
        toggler.HideInstant(disconnectPanel);
        toggler.HideInstant(controllerPanel);
        connectPanel.SetActive(true);

        if (networkHost != null)
            networkHost.OnConnectionChanged += SetConnectedUI;

        if (networkClient != null)
            networkClient.OnConnectionChanged += SetConnectedUI;
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

        // Only clients (phones) get the controller panel toggled
        if (IsClient)
            toggler.ToggleActive(controllerPanel);
    }

    private void SetConnectedUI(bool isConnected)
    {
        toggler.HideInstant(connectPanel);
        toggler.HideInstant(disconnectPanel);
        toggler.HideInstant(controllerPanel);

        if (isConnected)
        {
            toggler.SlideIn(disconnectPanel, disconnectPanel.GetComponent<RectTransform>());

            // Only slide in controller panel for clients (phones), not the host
            if (IsClient)
                toggler.SlideIn(controllerPanel, controllerPanel.GetComponent<RectTransform>());
        }
        else
        {
            toggler.SlideIn(connectPanel, connectPanel.GetComponent<RectTransform>());
        }
    }

    private void OnDestroy()
    {
        if (networkHost != null)
            networkHost.OnConnectionChanged -= SetConnectedUI;

        if (networkClient != null)
            networkClient.OnConnectionChanged -= SetConnectedUI;
    }
}