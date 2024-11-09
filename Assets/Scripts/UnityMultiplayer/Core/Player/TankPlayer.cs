using Cinemachine;
using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class TankPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera _cinemachinecCamera;
    [field: SerializeField] public Health Health { get; private set; }
    [field: SerializeField] public CoinWallet Wallet { get; private set; }
    [SerializeField] private SpriteRenderer _miniMapIcon;
    [SerializeField] private Color _ownerColor;

    [Header("Settings")]
    [SerializeField] private int _cameraownerPriority = 11;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();
    public static Action<TankPlayer> OnPlayerSpawned;
    public static Action<TankPlayer> OnPlayerDespawned;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UserData userData =  HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            PlayerName.Value = userData.UserName;
            OnPlayerSpawned?.Invoke(this);
        }
        if (IsOwner)
        {
            if (_cinemachinecCamera != null)
            {
                _cinemachinecCamera.Priority = _cameraownerPriority;
            }
            
            if (_miniMapIcon != null)
            {
                _miniMapIcon.color = _ownerColor;
            }
        }
    }
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
        }
    }
}
