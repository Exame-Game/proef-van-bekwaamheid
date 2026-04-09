// On your Player prefab
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkSetup : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        PlayerCameraManager.Instance.RegisterPlayer(transform);
    }

    public override void OnNetworkDespawn()
    {
        PlayerCameraManager.Instance.UnregisterPlayer(transform);
    }
}