using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer : IDisposable
{
    private NetworkManager _networkManager;

    public Action<string> OnClientLeft;

    private Dictionary<ulong, string> _clientIdToAuth = new Dictionary<ulong, string>();
    private Dictionary<string, UserData> _authIdToUserData = new Dictionary<string, UserData>();

    public NetworkServer(NetworkManager networkManager)
    {
        _networkManager = networkManager;
        //Connect to the ConnectionApprovalCallback to listen to the player info being send
        _networkManager.ConnectionApprovalCallback += ApprobalCheck;
        _networkManager.OnServerStarted += OnNetworkReady;
    }

    // This function handles the players info being send
    private void ApprobalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        //Playload is a byte array so we converet that into a Json String
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        //take the Json string and cash it to a UserData
        UserData userData = JsonUtility.FromJson<UserData>(payload);
        _clientIdToAuth[request.ClientNetworkId] = userData.UserAuthId;
        _authIdToUserData[userData.UserAuthId] = userData;

        // this is to let them finish the connection to the server
        response.Approved = true;
        response.Position = SpawnPoint.GetRandomSpawnPosition();
        response.Rotation = Quaternion.identity;
        response.CreatePlayerObject = true;
    }

    private void OnNetworkReady()
    {
        _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (_clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            _clientIdToAuth.Remove(clientId);
            _authIdToUserData.Remove(authId);
            //Tells Host that a player has left the game 
            OnClientLeft?.Invoke(authId);
        }
       
    }

    public UserData GetUserDataByClientId(ulong clientId)
    {
        if (_clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            if (_authIdToUserData.TryGetValue(authId, out UserData data))
            {
                return data;
            }
        }
        return null;
    }

    public void Dispose()
    {
        if ( _networkManager == null ) return;
        _networkManager.ConnectionApprovalCallback -= ApprobalCheck;
        _networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        _networkManager.OnServerStarted -= OnNetworkReady;
        if (_networkManager.IsListening)
        {
            _networkManager.Shutdown();
        }
    }
}
