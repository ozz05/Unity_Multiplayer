using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkServer : IDisposable
{
    private NetworkManager _networkManager;
    private NetworkObject _playerPrefab;
    public Action<string> OnClientLeft;
    public Action<UserData> OnUserJoined;
    public Action<UserData> OnUserLeft;


    private Dictionary<ulong, string> _clientIdToAuth = new Dictionary<ulong, string>();
    private Dictionary<string, UserData> _authIdToUserData = new Dictionary<string, UserData>();

    public NetworkServer(NetworkManager networkManager)
    {
        _networkManager = networkManager;
        //Connect to the ConnectionApprovalCallback to listen to the player info being send
        _networkManager.ConnectionApprovalCallback += ApprobalCheck;
        _networkManager.OnServerStarted += OnNetworkReady;
    }

    public bool OpenConnection(string ip, int port)
    {
        UnityTransport transport = _networkManager.gameObject.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, (ushort)port);
        return _networkManager.StartServer();

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
        OnUserJoined?.Invoke(userData);

        _ = SpawnPlayerDelayed(request.ClientNetworkId);
        // this is to let them finish the connection to the server
        response.Approved = true;
        response.Position = SpawnPoint.GetRandomSpawnPosition();
        response.Rotation = Quaternion.identity;
        response.CreatePlayerObject = true;
    }

    private async Task SpawnPlayerDelayed(ulong clientId)
    {
        await Task.Delay(1000);

        NetworkObject playerInstance =
            GameObject.Instantiate(_playerPrefab, SpawnPoint.GetRandomSpawnPosition(), Quaternion.identity);

        playerInstance.SpawnAsPlayerObject(clientId);
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
            OnUserLeft?.Invoke(_authIdToUserData[authId]);
            _authIdToUserData.Remove(authId);
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
