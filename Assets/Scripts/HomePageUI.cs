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
    public GameObject startPopUp;
    public GameObject joinGamePopup;
    public GameObject chooseTypePopUp;
    public GameObject CreateGamePopup;
    public Button startGameButton;
    public Button resetGameButton;
    public Button createGameButton;
    public Button joinGameButton;
    public Button playGameButton;
    public Button StopButton;
    public Button confirmJoinButton;
    public Button backToHomeButton;
    public Button hunterButton;
    public Button morphButton;
    public TMP_InputField gameCodeInputField;
    public TMP_InputField hostGameCodeInputField;
    public TMP_InputField playerNameInputField;
    public Canvas mainCanvas;
    public static HomePageUI Instance;
    public enum Type
    {
        Hunter,
        Morph
    }
    public Type type;

    private bool isCameraSwapped = false;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            createGameButton.onClick.AddListener(OnCreateGameClicked);
            joinGameButton.onClick.AddListener(OnJoinGameClicked);
            confirmJoinButton.onClick.AddListener(OnConfirmJoinClicked);
            backToHomeButton.onClick.AddListener(OnBackToHomeClicked);
            playGameButton.onClick.AddListener(ChooseType);
            hunterButton.onClick.AddListener(HunterSelected);
            morphButton.onClick.AddListener(MorphSelected);
            StopButton.onClick.AddListener(StopRelay);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    async void Start()
    {
        // Ensure the canvas is visible at the start
        mainCanvas.gameObject.SetActive(true);
        CreateGamePopup.SetActive(false); // Hide the popup at start
        joinGamePopup.SetActive(false); // Hide the popup at start
        chooseTypePopUp.gameObject.SetActive(false); // Hide the popup at start
        startPopUp.SetActive(true);

        await UnityServices.InitializeAsync();

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    void OnCreateGameClicked()
    {
        Debug.Log("Going to lobby scene");
        StartRelay();
        PlayerPrefs.SetString("PlayerName", playerNameInputField.text);
        CreateGamePopup.SetActive(true);
        startPopUp.SetActive(false);
        // Hide playerNameInputField
        playerNameInputField.gameObject.SetActive(false);
        GameManager.Instance.HandleGameButtons(startGameButton, resetGameButton, true);
    }

    void OnJoinGameClicked()
    {
        startPopUp.SetActive(false);
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
            await JoinRelay(gameCode);

            GameManager.Instance.HandleGameButtons(startGameButton, resetGameButton, false);
        }
        else
        {
            Debug.Log("Game code is empty!");
        }
    }

    void OnBackToHomeClicked()
    {
        startPopUp.SetActive(true);
        joinGamePopup.SetActive(false);
    }

    void HunterSelected()
    {
        Selection(Type.Hunter.ToString());
    }
    void MorphSelected()
    {
        Selection(Type.Morph.ToString());
    }

    void Selection(string type)
    {
        GameManager.Instance.type = type;

        PlayGame();
    }

    private async void StartRelay()
    {
        string joinCode = await StartHostWithRelay();

        Debug.Log("ICI: " + joinCode);
        //hostGameCodeInputField.readOnly = false;
        hostGameCodeInputField.text = joinCode;
        //hostGameCodeInputField.readOnly = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


    }

    private async Task<bool> JoinRelay(string joinCode)
    {
        bool connected = await StartClientWithRelay(joinCode);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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

    public void ChooseType()
    {
        createGameButton.gameObject.SetActive(false);
        CreateGamePopup.SetActive(false);
        joinGamePopup.SetActive(false);
        startPopUp.SetActive(false);
        chooseTypePopUp.SetActive(true);
    }

    public void PlayGame()
    {
        Debug.Log("ININININ");
        mainCanvas.gameObject.SetActive(false);
        chooseTypePopUp.SetActive(false);
        joinGamePopup.SetActive(false);
        startPopUp.SetActive(false);
        CreateGamePopup.SetActive(false);
        if (!isCameraSwapped)
        {
            isCameraSwapped = true;
            GameManager.Instance.SwapCamera();
        }
    }

    void StopRelay()
    {
        // Should stop the relay server ,disconnecting all clients and go back the main menu
    }

    public void ChangeType()
    {
        mainCanvas.gameObject.SetActive(true);
        CreateGamePopup.SetActive(false); // Hide the popup at start
        joinGamePopup.SetActive(false); // Hide the popup at start
        startPopUp.SetActive(false);
        chooseTypePopUp.gameObject.SetActive(false); // Hide the popup at start

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ChooseType();
    }
}
