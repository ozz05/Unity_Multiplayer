using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager
{
    private const string MenuSceneName = "Menu";
    private JoinAllocation _joinAllocation;
    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();
        AuthState authState = await AuthenticationWrapper.DoAuth();
        if (authState == AuthState.Authenticated)
        {
            return true;
        }
        return false;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(MenuSceneName);
    }

    public async Task StartClientAsync(string joinCode)
    {
        try
        {
            _joinAllocation = await Relay.Instance.JoinAllocationAsync(joinCode);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
            return;
        }
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = new RelayServerData(_joinAllocation, "dtls");//udp is another option
        transport.SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartClient();
    }
}
