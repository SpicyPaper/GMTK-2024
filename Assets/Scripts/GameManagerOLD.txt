using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public Transform[] spawnPoints; // Array of spawn points in the scene

    private List<PlayerController> players = new List<PlayerController>(); // List to track all players

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
        PlayerController.OnPlayerSpawned += RegisterPlayer;
        PlayerController.OnPlayerDespawned += UnregisterPlayer;
    }

    void OnDestroy()
    {
        // Unregister the event handlers to avoid memory leaks
        PlayerController.OnPlayerSpawned -= RegisterPlayer;
        PlayerController.OnPlayerDespawned -= UnregisterPlayer;
    }

    private void RegisterPlayer(PlayerController player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
        }
    }

    private void UnregisterPlayer(PlayerController player)
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
