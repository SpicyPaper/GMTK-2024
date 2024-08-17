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
using UnityEngine;
using UnityEngine.UI;

public class RelayManager : MonoBehaviour
{
    [SerializeField] private Text joinCodeText;
    [SerializeField] private InputField codeInput;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button joinBtn;
    // Start is called before the first frame update
    async void Start()
    {
        await UnityServices.InitializeAsync();

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        hostBtn.onClick.AddListener(() =>
        {
            StartRelay();
        });

        joinBtn.onClick.AddListener(() =>
        {
            JoinRelay();
        });


    }

    private async void StartRelay()
    {
        string joinCode = await StartHostWithRelay();
        joinCodeText.text = joinCode;

        Debug.Log(joinCode);
    }

    private async void JoinRelay()
    {
        await StartClientWithRelay(codeInput.text);
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
}
