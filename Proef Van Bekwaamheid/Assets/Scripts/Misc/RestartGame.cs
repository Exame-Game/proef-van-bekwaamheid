using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class RestartGame : MonoBehaviour
{
    public ConnectionManager connectionManager;
    public ItemSpawner itemSpawner;
    public ItemCollection itemCollection;

    public void RestartGameScene()
    {
        StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        if (NetworkManager.Singleton.IsServer)
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
                if (client.PlayerObject != null)
                    client.PlayerObject.Despawn();

        NetworkManager.Singleton.Shutdown();

        yield return new WaitUntil(() => !NetworkManager.Singleton.IsListening);

        connectionManager.InitializeHostAndClient();

        yield return new WaitUntil(() => NetworkManager.Singleton.IsServer);

        Debug.Log("<color=cyan>[RestartGame] Server restarted successfully. Spawning items...</color>");

        itemSpawner.ResetItems();
        itemSpawner.Initialize();

        InventoryData.Instance.ResetInventory();
        itemCollection.UpdateInventoryText();
    }
}
