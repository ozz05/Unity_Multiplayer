using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ServerGameManager : IDisposable
{
    private string _serverIP;
    private int _serverPort;
    private int _queryPort;
    private MatchplayBackfiller _backfiller;
    private  NetworkServer _networkServer;
    private MultiplayAllocationService _multiplayAllocationService;
    private const string GameSceneName = "Game";

    public ServerGameManager(string serverIP, int serverPort, int queryPort, NetworkManager networkManager)
    {
        _serverIP = serverIP;
        _serverPort = serverPort;
        _queryPort = queryPort;
        _networkServer = new NetworkServer(networkManager);
        _multiplayAllocationService = new MultiplayAllocationService();
    }
    public async Task StartGameServerAsync()
    {
        await _multiplayAllocationService.BeginServerCheck();

        try 
        {
            MatchmakingResults matchmakerPayload = await GetMatchmakingPayload();
            if(matchmakerPayload != null)
            {
                await StartBackfill(matchmakerPayload);
                _networkServer.OnUserJoined += UserJoined;
                _networkServer.OnUserLeft += UserLeft;

            }
            else
            {
                Debug.LogWarning("Matchmaker payload timed out");
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        if(!_networkServer.OpenConnection(_serverIP, _serverPort))
        {
            Debug.LogError("NetworkServer did not start as expected");
            return;
        }
        //Loads the scene
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    private async Task<MatchmakingResults> GetMatchmakingPayload()
    {
        Task<MatchmakingResults> matchmakerPayloadTask = _multiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();

        if(await Task.WhenAny(matchmakerPayloadTask, Task.Delay(20000)) == matchmakerPayloadTask)
        {
            return matchmakerPayloadTask.Result;
        }

        return null;
    }

    private async Task StartBackfill(MatchmakingResults matchmakerPayload)
    {
        _backfiller = new MatchplayBackfiller($"{_serverIP}:{_serverPort}",
            matchmakerPayload.QueueName,
            matchmakerPayload.MatchProperties,
            20);
        if(_backfiller.NeedsPlayers())
        {
            await _backfiller.BeginBackfilling();
        }
    }

    private void UserJoined(UserData user)
    {
        _backfiller.AddPlayerToMatch(user);
        _multiplayAllocationService.AddPlayer();
        if (!_backfiller.NeedsPlayers() && _backfiller.IsBackfilling)
        {
            _ = _backfiller.StopBackfill();
        }
    }

    private void UserLeft(UserData user)
    {
        int playerCount = _backfiller.RemovePlayerFromMatch(user.UserAuthId);
        _multiplayAllocationService.RemovePlayer();

        if (playerCount <= 0)
        {
            CloseServer();
            return;
        }

        if (_backfiller.NeedsPlayers() && !_backfiller.IsBackfilling)
        {
            _ = _backfiller.BeginBackfilling();
        }
    }

    private async void CloseServer()
    {
        await _backfiller.StopBackfill();
        Dispose();
        Application.Quit();
    }


    public void Dispose()
    {
        _networkServer.OnUserJoined -= UserJoined;
        _networkServer.OnUserLeft -= UserLeft;

        _backfiller?.Dispose();

        _multiplayAllocationService?.Dispose();
        _networkServer?.Dispose();
    }
}
