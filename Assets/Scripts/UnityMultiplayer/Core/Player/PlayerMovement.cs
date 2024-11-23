using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _bodyTransform;
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private ParticleSystem _dustCloud;


    [Header("Settings")]
    [SerializeField] private float _movementSpeed = 4f;
    [SerializeField] private float _turningRate = 30f;
    [SerializeField] private float _particleEmisionValue = 10;

    private const float ParticleStopThreshold = 0.0005f;
    private ParticleSystem.EmissionModule _emissionModule;
    private Vector2 _previousMovementInput;
    private Vector3 _previousPos;

    private void Awake()
    {
        _emissionModule = _dustCloud.emission;
    }
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
    private void UpdateDustParticles()
    {
        if ((transform.position - _previousPos).sqrMagnitude > ParticleStopThreshold)
        {
            _emissionModule.rateOverTime = _particleEmisionValue;
        }
        else
        {
            _emissionModule.rateOverTime = 0;
        }
    }
    private void Update()
    {
        UpdateDustParticles();
        _previousPos = transform.position;
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
