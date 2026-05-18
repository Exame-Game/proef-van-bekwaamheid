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
        yield return new WaitUntil(() => !NetworkManager.Singleton.IsListening);

        connectionManager.InitializeHostAndClient();

        yield return new WaitUntil(() => NetworkManager.Singleton.IsServer);

        itemSpawner.Initialize();
    }
}