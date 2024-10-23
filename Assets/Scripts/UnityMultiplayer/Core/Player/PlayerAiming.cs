using Unity.Netcode;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _turretTransform;

    private void LateUpdate() 
    {
        if (!IsOwner) return;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(_inputReader.AimPosition);
        _turretTransform.up = new Vector2(
            mousePosition.x - _turretTransform.position.x,
            mousePosition.y - _turretTransform.position.y
        );
    }
}
