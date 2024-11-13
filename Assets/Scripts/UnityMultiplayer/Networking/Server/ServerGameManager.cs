using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerGameManager : IDisposable
{
    private string _serverIP;
    private int _serverPort;
    private int _queryPort;
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

        if(!_networkServer.OpenConnection(_serverIP, _serverPort))
        {
            Debug.LogError("NetworkServer did not start as expected");
            return;
        }
        //Loads the scene
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    public void Dispose()
    {
        _multiplayAllocationService?.Dispose();
        _networkServer?.Dispose();
    }
}
