using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField] private TankPlayer _playerPrefab;
    [SerializeField] private float _keptPointPercentage = 50;

    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;

        TankPlayer[] players =  FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);

        foreach( TankPlayer player in players )
        {
            HandlePlayerSpawned( player );
        }

        TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
    }
    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
    }

    private void HandlePlayerSpawned(TankPlayer player)
    {
        player.Health.onDeath += (health) => HanldePlayerDeath(player);
    }

    private void HandlePlayerDespawned(TankPlayer player)
    {
        //The onDeath Event send health as a parameter but doing this will allow you subscribe sending different parameters 
        player.Health.onDeath -= (health) => HanldePlayerDeath(player);
    }

    private void HanldePlayerDeath(TankPlayer player)
    {
        int  keptCoins = (int) (player.Wallet.TotalCoins.Value * (_keptPointPercentage / 100));
        Destroy(player.gameObject);

        //waits for a frame so the destroy function gets correctly executed
        StartCoroutine(RespawnPlayer(player.OwnerClientId, keptCoins));
    }

    private IEnumerator RespawnPlayer(ulong ownerClientId, int keptCoins)
    {
        yield return null;
        // Creates a new instance of a player
        TankPlayer playerInstance =  Instantiate(
            _playerPrefab, SpawnPoint.GetRandomSpawnPosition(), Quaternion.identity);
       
        // asigns that instance to the client so it can belong to it
        playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientId);
        //When modifying a network variable it should be after being spawn
        playerInstance.Wallet.TotalCoins.Value = keptCoins;
    }
}
