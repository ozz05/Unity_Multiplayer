using System;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class NetworkClient : IDisposable
{
    private NetworkManager _networkManager;
    private const string MenuSceneName = "Menu";
    public NetworkClient(NetworkManager networkManager)
    {
        _networkManager = networkManager;
        //Connect to the ConnectionApprovalCallback to listen to the player info being send
        _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        //Make sure that the client is being disconnected from the server(the server shut down)
        if (clientId != 0 && clientId != _networkManager.LocalClientId) { return; }

        //Check that the client isn't in the menu scene
        if (SceneManager.GetActiveScene().name != MenuSceneName)
        {
            SceneManager.LoadScene(MenuSceneName);
        }
        // checks if the client is still connected to a server, it disconnects it
        if (_networkManager.IsConnectedClient)
        {
            _networkManager.Shutdown();
        }
    }

    public void Dispose()
    {
        if (_networkManager != null)
        {
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }
}
