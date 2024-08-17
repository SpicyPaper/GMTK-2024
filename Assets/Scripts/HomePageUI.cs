using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class HomePageUI : MonoBehaviour
{
    public GameObject joinGamePopup;
    public GameObject CreateGamePopup;
    public Button createGameButton;
    public Button joinGameButton;
    public Button playGameButton;
    public Button StopButton;
    public Button confirmJoinButton;
    public Button backToHomeButton;
    public TMP_InputField gameCodeInputField;
    public TMP_InputField hostGameCodeInputField;
    public TMP_InputField playerNameInputField;
    public Canvas mainCanvas;
    public Canvas debugCanvas;

    async void Start()
    {
        // Ensure the canvas is visible at the start
        mainCanvas.gameObject.SetActive(true);
        debugCanvas.gameObject.SetActive(false);
        CreateGamePopup.SetActive(false); // Hide the popup at start
        joinGamePopup.SetActive(false); // Hide the popup at start

        await UnityServices.InitializeAsync();

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        createGameButton.onClick.AddListener(OnCreateGameClicked);
        joinGameButton.onClick.AddListener(OnJoinGameClicked);
        confirmJoinButton.onClick.AddListener(OnConfirmJoinClicked);
        backToHomeButton.onClick.AddListener(OnBackToHomeClicked);
        playGameButton.onClick.AddListener(PlayGame);
        StopButton.onClick.AddListener(StopRelay);

    }

    void OnCreateGameClicked()
    {
        Debug.Log("Going to lobby scene");
        StartRelay();
        PlayerPrefs.SetString("PlayerName", playerNameInputField.text);
        CreateGamePopup.SetActive(true);
        // Hide playerNameInputField
        playerNameInputField.gameObject.SetActive(false);
    }

    void OnJoinGameClicked()
    {
        joinGamePopup.SetActive(true);
        playerNameInputField.gameObject.SetActive(false);
    }

    async void OnConfirmJoinClicked()
    {
        string gameCode = gameCodeInputField.text;
        if (!string.IsNullOrEmpty(gameCode))
        {
            // Implement the logic for joining a game with the provided code
            Debug.Log("Joining game with code: " + gameCode);
            if (playerNameInputField.text != null) PlayerPrefs.SetString("PlayerName", playerNameInputField.text);
            else
            {
                string myuuidAsString = Guid.NewGuid().ToString();
                PlayerPrefs.SetString("PlayerName", "Player" + myuuidAsString);

            }
            bool connected = await JoinRelay(gameCode);

            if (connected)
            {
                PlayGame();
            }

            }
        else
        {
            Debug.Log("Game code is empty!");
        }
    }

    void OnBackToHomeClicked()
    {
        joinGamePopup.SetActive(false);
    }

    private async void StartRelay()
    {
        string joinCode = await StartHostWithRelay();

        Debug.Log("ICI: " + joinCode);
        //hostGameCodeInputField.readOnly = false;
        hostGameCodeInputField.text = joinCode;
        //hostGameCodeInputField.readOnly = true;
    }

    private async Task<bool> JoinRelay(string joinCode)
    {
        bool connected = await StartClientWithRelay(joinCode);

        return connected;
    }

    // Update is called once per frame
    private async Task<string> StartHostWithRelay(int maxConnection = 8)
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnection);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }

    private async Task<bool> StartClientWithRelay(string joinCode)
    {
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }

    
    void PlayGame()
    {
        mainCanvas.gameObject.SetActive(false);
        debugCanvas.gameObject.SetActive(true);
        joinGamePopup.SetActive(false);
        CreateGamePopup.SetActive(false);
        //debugCanvas.GetComponent<SpawnManager>().Initialize();
        GameManager.Instance.InitHost();
        // Spawn the player
    }

    void StopRelay()
    {
        // Should stop the relay server ,disconnecting all clients and go back the main menu
    }


}
