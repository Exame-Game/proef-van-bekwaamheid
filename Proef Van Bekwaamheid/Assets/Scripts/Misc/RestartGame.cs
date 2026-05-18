using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class RestartGame : MonoBehaviour
{
    public ConnectionManager connectionManager;
    public ItemSpawner itemSpawner;

    public void RestartGameScene()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();

        StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        // Wait for Netcode to fully shut down
        yield return new WaitUntil(() => !NetworkManager.Singleton.IsListening);

        connectionManager.InitializeHostAndClient();

        // Wait until host is running before spawning
        yield return new WaitUntil(() => NetworkManager.Singleton.IsServer);

        itemSpawner.Initialize();
    }
}