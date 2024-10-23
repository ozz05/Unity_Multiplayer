using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _bodyTransform;
    [SerializeField] private Rigidbody2D _rigidbody;

    [Header("Settings")]
    [SerializeField] private float _movementSpeed = 4f;
    [SerializeField] private float _turningRate = 30f;

    private Vector2 _previousMovementInput;

    public override void OnNetworkSpawn()
    {
        if(!IsOwner) {return;}
        _inputReader.MoveEvent += HandleMove;
    }

    public override void OnNetworkDespawn()
    {
        if(!IsOwner) {return;}
        _inputReader.MoveEvent -= HandleMove; 
    }
    private void Update()
    {
        if(!IsOwner) {return;}
        float zRotation = _previousMovementInput.x * -_turningRate * Time.deltaTime;
        _bodyTransform.Rotate(0, 0, zRotation);
    }

    private void FixedUpdate()
    {
        if(!IsOwner) {return;}
        
        _rigidbody.velocity = (Vector2)_bodyTransform.up * _previousMovementInput.y * _movementSpeed;
    }

    private void HandleMove(Vector2 movementInput)
    {
        _previousMovementInput = movementInput;
    }
}
