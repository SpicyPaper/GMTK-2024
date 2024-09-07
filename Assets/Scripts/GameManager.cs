using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public Transform[] spawnPoints; // Array of spawn points in the scene
    // Reference to the main camera in the scene
    public Camera mainCamera;
    private Camera playerCamera;

    public HomePageUI.Type type;

    private List<PlayerInteraction> players = new List<PlayerInteraction>(); // List to track all players

    public GameObject localPlayer;

    public NetworkVariable<bool> GameStarted = new NetworkVariable<bool>(false);

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

    public void Update()
    {
        // If the player presses the "C" key, unlock the cursor
        if (Input.GetKeyDown(KeyCode.C))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void HandleGameButtons(Button start, Button reset, bool isServer)
    {
        if (isServer)
        {
            Debug.Log("Assigning the button !");
            reset.onClick.AddListener(ResetGame);
            start.onClick.AddListener(StartRound);
        }
        else
        {
            reset.gameObject.SetActive(false);
            start.gameObject.SetActive(false);
        }
    }

    public void SetPlayerCamera(Camera camera)
    {
        playerCamera = camera;
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

    public void ResetGame()
    {
        // Logic to start a new round, respawn players, etc.
        // For example:
        Debug.Log("Reseting game");
        NetworkManager.Singleton.SceneManager.LoadScene("AlexScene", LoadSceneMode.Single);
        RespawnAllPlayersServerRpc();
    }

    public void StartRound()
    {
        Debug.Log("Starting game");
        GameStarted.Value = true;
        RespawnAllPlayersServerRpc();
    }

    [ServerRpc]
    private void RespawnAllPlayersServerRpc()
    {
        for (int i = 0; i < players.Count; i++)
        {
            int rand = Random.Range(0, spawnPoints.Length);

            Vector3 pos = spawnPoints[rand].position; // Assign spawn point in a round-robin fashion
            players[i].SpawnPoint.Value = pos;
        }
        RespawnClientRpc();
    }

    [ClientRpc]
    public void RespawnClientRpc()
    {
        Debug.Log("Starting respawn");

        for (int i = 0; i < players.Count; i++)
        {
            players[i].Respawn();
        }
    }

    public void SwapCamera()
    {
        Debug.Log("Swapping camera");
        mainCamera.enabled = !mainCamera.enabled;
        playerCamera.enabled = !playerCamera.enabled;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
