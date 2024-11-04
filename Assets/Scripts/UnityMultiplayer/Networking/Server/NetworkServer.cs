using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer
{
    private NetworkManager _networkManager;

    private Dictionary<ulong, string> _clientIdToAuth = new Dictionary<ulong, string>();
    private Dictionary<string, UserData> _authIdToUserData = new Dictionary<string, UserData>();
    public NetworkServer(NetworkManager networkManager)
    {
        _networkManager = networkManager;
        //Connect to the ConnectionApprovalCallback to listen to the player info being send
        networkManager.ConnectionApprovalCallback += ApprobalCheck;
        networkManager.OnServerStarted += OnNetworkReady;
    }

    // This function handles the players info being send
    private void ApprobalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        //Playload is a byte array so we converet that into a Json String
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        //take the Json string and cash it to a UserData
        UserData userData = JsonUtility.FromJson<UserData>(payload);
        _clientIdToAuth[request.ClientNetworkId] = userData.UserAuthID;
        _authIdToUserData[userData.UserAuthID] = userData;

        // this is to let them finish the connection to the server
        response.Approved = true;
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
        }
       
    }
}
