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

public class ClientGameManager : IDisposable 
{
    private JoinAllocation _joinAllocation;
    private NetworkClient _networkClient;
    private MatchplayMatchmaker _matchmaker;
    private UserData _userData;

    private const string MenuSceneName = "Menu";
    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();
        
        //This initializes the NetworkCLient class to start listening to when the client gets disconnected from the server
        _networkClient = new NetworkClient(NetworkManager.Singleton);
        _matchmaker = new MatchplayMatchmaker();

        AuthState authState = await AuthenticationWrapper.DoAuth();
        if (authState == AuthState.Authenticated)
        {
            //Create UsereData
            _userData = new UserData
            {
                UserName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
                UserAuthId = AuthenticationService.Instance.PlayerId,

            };
            return true;
        }
        return false;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(MenuSceneName);
    }

    public void StartClient(string ip, int port)
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, (ushort)port);
        ConnectClient();
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

        ConnectClient();
    }

    private void ConnectClient()
    {
        //Convert to Json
        string payLoad = JsonUtility.ToJson(_userData);
        //Make byte array
        byte[] payLoadBytes = Encoding.UTF8.GetBytes(payLoad);
        //Send it to server
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payLoadBytes;

        //Start client
        NetworkManager.Singleton.StartClient();
    }
    public async void MatchmakeAsync(Action<MatchmakerPollingResult> onMatchmakeResponse)
    {
        if(_matchmaker.IsMatchmaking)
        {
            return;
        }
        MatchmakerPollingResult result = await GetMatchAsync();
        onMatchmakeResponse?.Invoke(result);
    }
    private async Task<MatchmakerPollingResult> GetMatchAsync()
    {
        MatchmakingResult matchmakingResult = await _matchmaker.Matchmake(_userData);

        if(matchmakingResult.result == MatchmakerPollingResult.Success)
        {
            StartClient(matchmakingResult.ip, matchmakingResult.port);
        }
        
        return matchmakingResult.result;
    }

    public async Task CancelMatchmaking()
    {
        await _matchmaker.CancelMatchmaking();
    }

    public void Disconnect()
    {
        _networkClient.Disconnect();
    }
    public void Dispose()
    {
        _networkClient?.Dispose();
    }

    
}
