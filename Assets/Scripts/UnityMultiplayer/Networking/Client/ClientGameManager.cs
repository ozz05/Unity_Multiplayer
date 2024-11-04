using System;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager
{
    private JoinAllocation _joinAllocation;
    private NetworkClient _networkClient;

    private const string MenuSceneName = "Menu";
    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();
        
        //This initializes the NetworkCLient class to start listening to when the client gets disconnected from the server
        _networkClient = new NetworkClient(NetworkManager.Singleton);

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

        //Create UsereData
        UserData userData = new UserData
        {
            UserName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
            UserAuthID = AuthenticationService.Instance.PlayerId,

        };
        //Convert to Json
        string payLoad = JsonUtility.ToJson(userData);
        //Make byte array
        byte[] payLoadBytes = Encoding.UTF8.GetBytes(payLoad);
        //Send it to server
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payLoadBytes;

        //Start client
        NetworkManager.Singleton.StartClient();
    }
}
