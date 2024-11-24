using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private TankPlayer _player;
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
    private bool _isPointerOverUI;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        _inputReader.PrimariFireEvent += HandlePrimaryFire;
    }

    private void HandlePrimaryFire(bool shouldFire)
    {
        if(_shouldFire)
        {
            if(_isPointerOverUI) { return; }
        }
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
        //Check if the pointer is over any UI
        _isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
        if (!_shouldFire) return;
        if ((Time.time - _previousFireTime) < _fireRate) return;

        _previousFireTime = Time.time;
        if (!_coinWallet.CanSpendCoins(_costToFire)) return;
        PrimaryFireServerRpc(_projectileSpawnPoint.position, _projectileSpawnPoint.up);
        SpawnDummyProjectile(_projectileSpawnPoint.position, _projectileSpawnPoint.up, _player.TeamIndex.Value);
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPosition, Vector3 direction)
    {
        if (!_coinWallet.CanSpendCoins(_costToFire)) return;
        
        _coinWallet.SpendCoins(_costToFire);

        GameObject projectileInstance = Instantiate(
            _serverProjectilePrefab, 
            spawnPosition, 
            Quaternion.identity
        );

        projectileInstance.transform.up = direction;
        Physics2D.IgnoreCollision(_playerCollider, projectileInstance.GetComponent<Collider2D>());
        InitializeProjectile(projectileInstance, _player.TeamIndex.Value);
        AddProjectileSpeed(projectileInstance);
        SpawnDummyProjectileClientRpc(spawnPosition, direction, _player.TeamIndex.Value);
    }

    [ClientRpc]
    private void SpawnDummyProjectileClientRpc(Vector3 spawnPosition, Vector3 direction, int teamIndex)
    {
        if (IsOwner) return;
        SpawnDummyProjectile(spawnPosition, direction, teamIndex);
    }
    private void InitializeProjectile(GameObject projectileInstance, int teamIndex)
    {
        if (projectileInstance.TryGetComponent<Projectile>(out Projectile projectile))
        {
            projectile.Initialize(teamIndex);
        }
    }
    private void AddProjectileSpeed(GameObject projectileInstance)
    {
        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = rb.transform.up * _projectileSpeed; 
        }
    }
    private void SpawnDummyProjectile(Vector3 spawnPosition, Vector3 direction, int teamIndex)
    {
        _muzzleFlash.SetActive(true);
        _muzzleFlashTimer = _muzzleFlashDuration;
        GameObject projectileInstance = Instantiate(
            _clientProjectilePrefab, 
            spawnPosition, 
            Quaternion.identity
        );

        projectileInstance.transform.up = direction;
        Physics2D.IgnoreCollision(_playerCollider, projectileInstance.GetComponent<Collider2D>());
        AddProjectileSpeed(projectileInstance);
        InitializeProjectile(projectileInstance, teamIndex);
    }
}
