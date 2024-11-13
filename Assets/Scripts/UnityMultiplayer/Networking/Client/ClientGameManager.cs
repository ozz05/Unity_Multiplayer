using System;
using System.Collections;
using System.Collections.Generic;
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
    public UserData UserData { get; private set; }

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
            UserData = new UserData
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
        string payload = JsonUtility.ToJson(UserData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartClient();
    }

    public async void MatchmakeAsync(Action<MatchmakerPollingResult> onMatchmakerResponse)
    {
        if(_matchmaker.IsMatchmaking) return;
        MatchmakerPollingResult result = await GetMatchAsync();
        onMatchmakerResponse?.Invoke(result);
    }

    private async Task<MatchmakerPollingResult> GetMatchAsync()
    {
        MatchmakingResult matchmakingResult = await _matchmaker.Matchmake(UserData);

        if (matchmakingResult.result == MatchmakerPollingResult.Success)
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
