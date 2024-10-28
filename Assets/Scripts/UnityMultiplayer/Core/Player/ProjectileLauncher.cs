using Unity.Netcode;
using UnityEngine;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _projectileSpawnPoint;
    [SerializeField] private GameObject _serverProjectilePrefab;
    [SerializeField] private GameObject _clientProjectilePrefab;
    [SerializeField] private GameObject _muzzleFlash;
    [SerializeField] private Collider2D _playerCollider;
    [SerializeField] private CoinWallet _coinWallet;

    [Header("Settings")]
    [SerializeField] private float _projectileSpeed;
    [SerializeField] private float _fireRate;
    [SerializeField] private float _muzzleFlashDuration;
    [SerializeField] private int _costToFire;
    private float _previousFireTime;
    private float _muzzleFlashTimer;
    private bool _shouldFire;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        _inputReader.PrimariFireEvent += HandlePrimaryFire;
    }

    private void HandlePrimaryFire(bool shouldFire)
    {
        _shouldFire = shouldFire;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        _inputReader.PrimariFireEvent -= HandlePrimaryFire;
    }
    private void Update()
    {
        if(_muzzleFlashTimer > 0f)
        {
            _muzzleFlashTimer -= Time.deltaTime;
            if (_muzzleFlashTimer <= 0f)
            {
                _muzzleFlash.SetActive(false);
            }
        }

        if (!IsOwner) return;
        if (!_shouldFire) return;
        if ((Time.time - _previousFireTime) < _fireRate) return;

        _previousFireTime = Time.time;
        if (!_coinWallet.CanSpendCoins(_costToFire)) return;
        PrimaryFireServerRpc(_projectileSpawnPoint.position, _projectileSpawnPoint.up);
        SpawnDummyProjectile(_projectileSpawnPoint.position, _projectileSpawnPoint.up);
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPosition, Vector3 direction)
    {
        if (!_coinWallet.CanSpendCoins(_costToFire)) return;
        
        _coinWallet.SpendCoins(_costToFire);

        GameObject projectile = Instantiate(
            _serverProjectilePrefab, 
            spawnPosition, 
            Quaternion.identity
        );

        projectile.transform.up = direction;
        if (projectile.TryGetComponent<DealDamageOnContact>(out DealDamageOnContact dealDamageOnContact))
        {
            dealDamageOnContact.SetOwner(OwnerClientId);
        }
        Physics2D.IgnoreCollision(_playerCollider, projectile.GetComponent<Collider2D>());
        AddProjectileSpeed(projectile);
        SpawnDummyProjectileClientRpc(spawnPosition, direction);
    }

    [ClientRpc]
    private void SpawnDummyProjectileClientRpc(Vector3 spawnPosition, Vector3 direction)
    {
        if (IsOwner) return;
        SpawnDummyProjectile(spawnPosition, direction);
    }
    private void AddProjectileSpeed(GameObject projectile)
    {
        if (projectile.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = rb.transform.up * _projectileSpeed; 
        }
    }
    private void SpawnDummyProjectile(Vector3 spawnPosition, Vector3 direction)
    {
        _muzzleFlash.SetActive(true);
        _muzzleFlashTimer = _muzzleFlashDuration;
        GameObject projectile = Instantiate(
            _clientProjectilePrefab, 
            spawnPosition, 
            Quaternion.identity
        );

        projectile.transform.up = direction;
        Physics2D.IgnoreCollision(_playerCollider, projectile.GetComponent<Collider2D>());
        AddProjectileSpeed(projectile);
    }
}
