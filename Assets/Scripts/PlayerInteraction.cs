using System;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;
using UnityEngine.XR;

public class PlayerInteraction : NetworkBehaviour
{
    public GameObject characterModel;
    public Transform spawnPoint; // Reference to the spawn point where the player should respawn
    public int maxHealth = 100;

    public static event Action<PlayerInteraction> OnPlayerSpawned;
    public static event Action<PlayerInteraction> OnPlayerDespawned;

    // Network variable to track the player's current health across the network
    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();

    void Start()
    {
        if (IsServer)
        {
            // Initialize the player's health on the server
            CurrentHealth.Value = maxHealth;
        }

        //if (IsOwner)
        //{
        //    // Add listener to the kill button if this is the local player
        //    UIManager.Instance.killButton.onClick.AddListener(KillCharacter);
        //}

        OnPlayerSpawned?.Invoke(this); // Notify that this player has spawned
    }

    private void OnDestroy()
    {
        OnPlayerDespawned?.Invoke(this); // Notify that this player has despawned
    }

    [ServerRpc]
    public void KillCharacterServerRpc()
    {
        // This method is called on the server, which then broadcasts the change to all clients
        KillCharacterClientRpc();
    }

    [ClientRpc]
    public void KillCharacterClientRpc()
    {
        // This method is called on all clients, and hides the character's model
        characterModel.SetActive(false);
    }

    public void KillCharacter()
    {
        // This method is called locally by the button, and then sends a request to the server to kill the character
        if (IsOwner)
        {
            KillCharacterServerRpc();
        }
    }

    [ClientRpc]
    public void RespawnClientRpc(Vector3 position)
    {
        // Set the character's position and make it visible again
        transform.position = position;
        characterModel.SetActive(true);
    }

    public void Respawn()
    {
        Debug.Log("Starting respawn");
        if (IsServer)
        {
            // Set the respawn position to the spawn point (or any other logic you want)
            Vector3 respawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;

            // Notify all clients to respawn this player
            RespawnClientRpc(respawnPosition);
        }
    }

    public void SetSpawnPoint(Transform newSpawnPoint)
    {
        // This method allows the GameManager to set the spawn point for this player
        spawnPoint = newSpawnPoint;
    }

    // ServerRpc method to apply damage to the player
    [ServerRpc]
    public void TakeDamageServerRpc(int damage)
    {
        if (IsServer)
        {
            // Decrease the player's health
            CurrentHealth.Value -= damage;

            // Check if the player's health has dropped to or below 0
            if (CurrentHealth.Value <= 0)
            {
                Die();
            }
        }
    }

    // Method to handle the player's death
    private void Die()
    {
        if (IsServer)
        {
            //// Optionally instantiate a death effect at the player's position
            //if (deathEffectPrefab != null)
            //{
            //    Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            //}

            // Notify all clients that this player has died
            DieClientRpc();
        }
    }

    // ClientRpc to handle the death on all clients
    [ClientRpc]
    private void DieClientRpc()
    {
        // Hide or disable the character model to simulate death
        characterModel.SetActive(false);
    }


}
