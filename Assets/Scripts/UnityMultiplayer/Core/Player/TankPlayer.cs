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
    [SerializeField] private Texture2D _crosshair;
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
            UserData userData = null;

            if (IsHost)
            {
                userData =
                    HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            }
            else
            {
                userData =
                    ServerSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            }

            PlayerName.Value = userData.UserName;
            OnPlayerSpawned?.Invoke(this);
        }
        if (IsOwner)
        {
            _cinemachinecCamera.Priority = _cameraownerPriority;
            
             _miniMapIcon.color = _ownerColor;
            Cursor.SetCursor(_crosshair, new Vector2(_crosshair.width/2, _crosshair.height/2), CursorMode.Auto);
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
