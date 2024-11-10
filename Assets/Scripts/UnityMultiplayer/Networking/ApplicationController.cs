using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] private ClientSingleton _clientPrefab;
    [SerializeField] private HostSingleton _hostPrefab;
    [SerializeField] private ServerSingleton _serverPrefab;
    private async void Start()
    {
        DontDestroyOnLoad(gameObject);

        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }

    private async Task LaunchInMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {
            ServerSingleton serverSingleton = Instantiate(_serverPrefab);
            await serverSingleton.CreateServer();

            await serverSingleton.GameManager.StartGameServerAsync();
        }
        else
        {
            HostSingleton hostSingleton = Instantiate(_hostPrefab);
            hostSingleton.CreateHost();

            ClientSingleton clientSingleton = Instantiate(_clientPrefab);
            bool authenticated = await clientSingleton.CreateClientAsync();
            
            //Go to main menu
            if (authenticated)
            {
                clientSingleton.GameManager.GoToMenu();
            }
        }
    }

}
