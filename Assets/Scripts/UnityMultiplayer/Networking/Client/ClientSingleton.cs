using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ClientSingleton : MonoBehaviour
{
    private static ClientSingleton _instance;
    private ClientGameManager _clientGameManager;

    public static ClientSingleton Instance 
    {
        get 
        {
            if (_instance != null) { return _instance; }
            _instance = FindObjectOfType<ClientSingleton>();
            if (_instance == null) 
            {
                Debug.LogError("No CLientSigleton in the scene");
                return null;
            }

            return _instance;
        }
    }
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public async Task CreateClientAsync()
    {
        _clientGameManager = new ClientGameManager();

        await _clientGameManager.InitAsync();
    }
}
