using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode; // Make sure you have the Netcode for GameObjects package installed

public class Spawner : MonoBehaviour
{
    public Button spawnButton; // Assign this in the Inspector
    public NetworkObject playerPrefab; // Change the type to NetworkObject

    private NetworkManager networkManager;

    void Start()
    {
        networkManager = NetworkManager.Singleton;

        if (spawnButton != null)
        {
            spawnButton.onClick.AddListener(SpawnAllPlayers);
        }
    }

    void SpawnAllPlayers()
    {
        if (!networkManager.IsHost)
        {
            Debug.Log("Only the host can spawn players.");
            return;
        }

        foreach (var client in networkManager.ConnectedClientsList)
        {
            NetworkObject playerInstance = Instantiate(playerPrefab); // Instantiate as NetworkObject
            playerInstance.SpawnAsPlayerObject(client.ClientId);
        }
    }
}
