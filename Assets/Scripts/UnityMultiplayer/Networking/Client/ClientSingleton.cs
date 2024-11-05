using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ClientSingleton : MonoBehaviour
{
    private static ClientSingleton _instance;
    public ClientGameManager GameManager {get; private set;}

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
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    public async Task<bool> CreateClientAsync()
    {
        GameManager = new ClientGameManager();

        return await GameManager.InitAsync();
    }
    private void OnDestroy()
    {
        GameManager?.Dispose();
    }
}
