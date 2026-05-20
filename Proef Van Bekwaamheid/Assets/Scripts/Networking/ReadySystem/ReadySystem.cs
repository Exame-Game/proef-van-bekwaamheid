using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;

public class ReadySystem : NetworkBehaviour
{
    private Dictionary<ulong, bool> playerReadyStatus = new Dictionary<ulong, bool>();
    private bool gameStarted = false;

    public delegate void OnAllPlayersReady();
    public event OnAllPlayersReady AllPlayersReadyEvent;

    public static ReadySystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null && IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!playerReadyStatus.ContainsKey(clientId))
        {
            playerReadyStatus[clientId] = false;
            Debug.Log($"Player {clientId} connected. Players needed to start: {playerReadyStatus.Count}");
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (playerReadyStatus.ContainsKey(clientId))
        {
            playerReadyStatus.Remove(clientId);
            Debug.Log($"Player {clientId} disconnected. Players needed to start: {playerReadyStatus.Count}");
        }
    }

    private void Update()
    {
    }

    [Rpc(SendTo.Server)]
    public void PlayerReadyRpc(bool isReady)
    {
        if (!IsServer) return;

        ulong clientId = NetworkManager.Singleton.LocalClientId;

        if (playerReadyStatus.ContainsKey(clientId))
        {
            playerReadyStatus[clientId] = isReady;

            int readyCount = GetReadyPlayerCount();
            int totalPlayers = playerReadyStatus.Count;

            if (isReady)
            {
                Debug.Log($"Player {clientId} is ready! ({readyCount}/{totalPlayers}) - Need {totalPlayers} players to start.");
            }
            else
            {
                Debug.Log($"Player {clientId} is not ready.");
            }

            // Broadcast updated ready status to all clients
            UpdateReadyStatusClientRpc(clientId, isReady);

            // Check if all players are ready
            if (AreAllPlayersReady())
            {
                StartGameServerRpc();
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateReadyStatusClientRpc(ulong clientId, bool isReady)
    {
        // Update UI or display on all clients
        Debug.Log($"Client-side update: Player {clientId} ready status: {isReady}");
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartGameServerRpc()
    {
        gameStarted = true;
        Debug.Log("All players are ready! Starting game...");
        AllPlayersReadyEvent?.Invoke();

        // Add your game start logic here
        // Example: Load game scene, spawn characters, etc.
    }

    private bool AreAllPlayersReady()
    {
        if (playerReadyStatus.Count == 0)
            return false;

        foreach (var status in playerReadyStatus.Values)
        {
            if (!status)
                return false;
        }

        return true;
    }

    private int GetReadyPlayerCount()
    {
        int count = 0;
        foreach (var status in playerReadyStatus.Values)
        {
            if (status) count++;
        }
        return count;
    }

    public int GetTotalPlayers()
    {
        return playerReadyStatus.Count;
    }

    public int GetReadyCount()
    {
        return GetReadyPlayerCount();
    }
}