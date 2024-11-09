using System;
using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Health _health;
    [SerializeField] private BountyCoin _coinPrefab;
    
    
    [Header("Settings")]
    [SerializeField] private float _coinSpread = 3f;
    [SerializeField] private float _bountyPercentage = 50f;
    [SerializeField] private int _bountyCoinsCount = 10;
    [SerializeField] private int _minBountyValue = 5;
    [SerializeField] private LayerMask _layerMask;

    private Collider2D[] _coinBuffer = new Collider2D[1];
    private float _coinRadius;

    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        _coinRadius = _coinPrefab.GetComponent<CircleCollider2D>().radius;

        _health.onDeath += HeandleDeath;
    }

    public override void OnNetworkDespawn()
    {
         if (!IsServer) return;
         _health.onDeath -= HeandleDeath;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!other.TryGetComponent<Coin>(out Coin coin)){return;}
        int coinValue = coin.Collect();
        if (!IsServer) return;
        TotalCoins.Value += coinValue;
    }
    public bool CanSpendCoins(int coinsToSpend)
    {
        return coinsToSpend <= TotalCoins.Value;
    }
    public void SpendCoins(int coinsToSpend)
    {

        if(CanSpendCoins(coinsToSpend))
        {
            if(IsServer)
            {
                TotalCoins.Value -= coinsToSpend;
            }
        }
    }

    private Vector2 GetSpawnPoint()
    {
        while (true)
        {
            //Gets a random position around the player
            Vector2 spawnPoint = (Vector2) transform.position + UnityEngine.Random.insideUnitCircle * _coinSpread;
            //Checks if the object is not going to be spawn in a safe position
            int numColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, _coinRadius, _coinBuffer, _layerMask);
            if (numColliders == 0) return spawnPoint;
        }
    }

    private void HeandleDeath(Health health)
    {
        int bountyValue = (int) (TotalCoins.Value * (_bountyPercentage / 100f));
        if (bountyValue < _minBountyValue) return;

        int bountyCoinValue = bountyValue / _bountyCoinsCount;
        for(int i = 0; i < _bountyCoinsCount; i++)
        {
            SpawnCoin(bountyCoinValue);
        }
    }

    private void SpawnCoin(int value)
    {
        BountyCoin coinInstance = Instantiate(
            _coinPrefab, 
            GetSpawnPoint(), 
            Quaternion.identity);
        coinInstance.SetValue(value);
        //Spawn the object in the network
        coinInstance.NetworkObject.Spawn();//We can do this becase coin inherits from NetworkBehavior
    }
}
