using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public Transform[] spawnPoints; // Array of spawn points in the scene

    private List<PlayerInteraction> players = new List<PlayerInteraction>(); // List to track all players

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitHost()
    {
        if (IsHost)
        {
            Debug.Log("Assigning the button !");
            UIManager.Instance.respawnButton.onClick.AddListener(StartNewRound);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Register the player to the GameManager when they spawn
        PlayerInteraction.OnPlayerSpawned += RegisterPlayer;
        PlayerInteraction.OnPlayerDespawned += UnregisterPlayer;
    }

    void OnDestroy()
    {
        // Unregister the event handlers to avoid memory leaks
        PlayerInteraction.OnPlayerSpawned -= RegisterPlayer;
        PlayerInteraction.OnPlayerDespawned -= UnregisterPlayer;
    }

    private void RegisterPlayer(PlayerInteraction player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
        }
    }

    private void UnregisterPlayer(PlayerInteraction player)
    {
        if (players.Contains(player))
        {
            players.Remove(player);
        }
    }

    public void StartNewRound()
    {
        // Logic to start a new round, respawn players, etc.
        // For example:
        Debug.Log("Starting a new round");
        RespawnAllPlayersServerRpc();
    }

    [ServerRpc]
    private void RespawnAllPlayersServerRpc()
    {
        for (int i = 0; i < players.Count; i++)
        {
            Transform assignedSpawnPoint = spawnPoints[i % spawnPoints.Length]; // Assign spawn point in a round-robin fashion
            players[i].SetSpawnPoint(assignedSpawnPoint);
            players[i].Respawn();
        }
    }
}
