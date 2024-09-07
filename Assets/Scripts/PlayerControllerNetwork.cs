using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System;

public class PlayerController : NetworkBehaviour
{
    public GameObject characterModel;
    public Transform spawnPoint; // Reference to the spawn point where the player should respawn

    public static event Action<PlayerController> OnPlayerSpawned;
    public static event Action<PlayerController> OnPlayerDespawned;

    void Start()
    {
        if (IsOwner)
        {
            // Add listener to the kill button if this is the local player
            UIManager.Instance.killButton.onClick.AddListener(KillCharacter);
        }
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

    private void Update()
    {
        if (!IsOwner) return;

        Vector3 moveDir = new Vector3(0, 0, 0);


        if (Input.GetKey(KeyCode.W)) moveDir.z += +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z += -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x += -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x += +1f;

        float moveSpeed = 3f;

        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }
}
