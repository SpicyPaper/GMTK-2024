using System;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;
using UnityEngine.XR;
using System.Collections.Generic;
using KinematicCharacterController.Examples;

public class PlayerInteraction : NetworkBehaviour
{
    public int maxHealth = 100;

    public static event Action<PlayerInteraction> OnPlayerSpawned;
    public static event Action<PlayerInteraction> OnPlayerDespawned;

    // Network variable to track the player's current health across the network
    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();
    public NetworkVariable<Vector3> SpawnPoint = new NetworkVariable<Vector3>(Vector3.zero);

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

    void OnDestroy()
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
        gameObject.SetActive(false);
    }

    public void KillCharacter()
    {
        // This method is called locally by the button, and then sends a request to the server to kill the character
        if (IsOwner)
        {
            KillCharacterServerRpc();
        }
    }

    public void Respawn()
    {
        gameObject.SetActive(true);
        // Set the character's position and make it visible again
        var ecc = gameObject.GetComponent<ExampleCharacterController>();
        ecc.Motor.SetPositionAndRotation(SpawnPoint.Value, transform.rotation);
        CurrentHealth.Value = maxHealth;
    }

    // ServerRpc method to apply damage to the player
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        Debug.Log("Player took dmg: " + OwnerClientId);
        // Decrease the player's health
        CurrentHealth.Value -= damage;

        // Check if the player's health has dropped to or below 0
        if (CurrentHealth.Value <= 0)
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
        gameObject.SetActive(false);
    }
}
