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
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;



public class HostGameManager : IDisposable
{
    private Allocation _allocation;
    private string _joinCode;
    private string _lobbyID;

    public NetworkServer NetworkServer { get; private set; }

    private const int MaxConnections = 20;
    private const string GameSceneName = "Game";
    public async Task StartHostAsync()
    {
        try
        {
            //Creates a server instance with a limit
            _allocation = await Relay.Instance.CreateAllocationAsync(MaxConnections);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
            return;
        }

        try
        {
            // gets the joinCode for players to join
            _joinCode = await Relay.Instance.GetJoinCodeAsync(_allocation.AllocationId);
            Debug.LogWarning("Join Code: " + _joinCode);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
            return;
        }


        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        RelayServerData relayServerData = new RelayServerData(_allocation, "dtls");//udp is another option
        transport.SetRelayServerData(relayServerData);

        //Create a lobby 
        try
        {
            //lobby options are settings for the new lobby being created, like if its private or not
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            // add data that can be read by other players where the visibility limits the access of that data
            lobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    //new data object that can only be access by the members of the lobby
                    "JoinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: _joinCode
                    )
                }
            };
            //Create Lobby
            string playerName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Unknown");
            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync(
                $"{playerName}'s Lobby", MaxConnections, lobbyOptions);
            _lobbyID = lobby.Id;
            HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15));
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
            return;
        }

        NetworkServer = new NetworkServer(NetworkManager.Singleton);

        //Create UsereData
        UserData userData = new UserData
        {
            UserName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
            UserAuthId = AuthenticationService.Instance.PlayerId,
        };
        //Convert to Json
        string payLoad = JsonUtility.ToJson(userData);
        //Make byte array
        byte[] payLoadBytes = Encoding.UTF8.GetBytes(payLoad);
        //Send it to server
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payLoadBytes;


        //start the host
        NetworkManager.Singleton.StartHost();

        NetworkServer.OnClientLeft += HandleOnClientLeft;

        //Loads the scene
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    //This coroutine pings the lobby service to let it know the lobby is still active if this doesn't happen this lobby instance will be deleted
    private IEnumerator HeartbeatLobby(float waitTimeSeconds)
    {
        // this is for performance so we do not create an new instance everytime
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(_lobbyID);
            yield return delay;
        }
    }

    public void Dispose()
    {
        Shutdown();
    }

    public async void Shutdown()
    {
        if (string.IsNullOrEmpty(_lobbyID)) return;
        //Stop the coroutine
        HostSingleton.Instance.StopCoroutine(nameof(HeartbeatLobby));
        try
        {
            await Lobbies.Instance.DeleteLobbyAsync(_lobbyID);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }
        NetworkServer?.Dispose();
        NetworkServer.OnClientLeft -= HandleOnClientLeft;
    }

    private async void HandleOnClientLeft(string authId)
    {
        try
        {
            //Removes the player from our lobby
            await LobbyService.Instance.RemovePlayerAsync(_lobbyID, authId);
        }
        catch(LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }
}
