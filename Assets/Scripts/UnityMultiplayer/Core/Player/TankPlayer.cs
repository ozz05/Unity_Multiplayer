using Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class TankPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera _cinemachinecCamera;

    [Header("Settings")]
    [SerializeField] private int _cameraownerPriority = 11;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
           UserData userData =  HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
           PlayerName.Value = userData.UserName;
        }
        if (IsOwner)
        {
            if (_cinemachinecCamera == null) return;
            _cinemachinecCamera.Priority = _cameraownerPriority;
        }
    }
}
