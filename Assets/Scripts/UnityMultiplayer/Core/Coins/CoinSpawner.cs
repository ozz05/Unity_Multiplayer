using Unity.Netcode;
using UnityEngine;

public class CoinSpawner : NetworkBehaviour
{
    [SerializeField] private RespawningCoin _coinPrefab;

    [Header("Settings")]
    [SerializeField] private int _maxCoins = 50;
    [SerializeField] private int _coinValue = 10;
    [SerializeField] private Vector2 _xSpawnRange;
    [SerializeField] private Vector2 _ySpawnRange;
    
    [SerializeField] private LayerMask _layerMask;
    private Collider2D[] _coinBuffer = new Collider2D[1];
    private float _coinRadius;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        _coinRadius = _coinPrefab.GetComponent<CircleCollider2D>().radius;
        for(int i = 0; i < _maxCoins; i ++)
        {
            SpawnCoin();
        }
    }
    private void SpawnCoin()
    {
        RespawningCoin coinInstance = Instantiate(
            _coinPrefab, 
            GetSpawnPoint(), 
            Quaternion.identity);
        coinInstance.SetValue(_coinValue);
        coinInstance.NetworkObject.Spawn();
        coinInstance.OnCollected += HandleCoinCollected;
    }

    private void HandleCoinCollected(RespawningCoin coin)
    {
        coin.transform.position = GetSpawnPoint();
        coin.Reset();
    }

    private Vector2 GetSpawnPoint()
    {
        Vector2 spawnPoint;
        float x = 0;
        float y = 0;
        while (true)
        {
            x = Random.Range(_xSpawnRange.x, _xSpawnRange.y);
            y = Random.Range(_ySpawnRange.x, _ySpawnRange.y);
            spawnPoint = new Vector2(x, y);
            int numColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, _coinRadius, _coinBuffer, _layerMask);
            if (numColliders == 0) return spawnPoint;
        }
    }
}
