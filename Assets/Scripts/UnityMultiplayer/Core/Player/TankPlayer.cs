using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TankPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera _cinemachinecCamera;

    [Header("Settings")]
    [SerializeField] private int _cameraownerPriority = 11;
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            if (_cinemachinecCamera == null) return;
            _cinemachinecCamera.Priority = _cameraownerPriority;
        }
    }
}
