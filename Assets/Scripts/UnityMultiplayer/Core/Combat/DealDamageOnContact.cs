using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : MonoBehaviour
{
    [SerializeField] private int _damge = 5;
    private ulong _ownerClientId;

    public void SetOwner(ulong ownerClientID)
    {
        _ownerClientId = ownerClientID;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.attachedRigidbody == null) return;
        if(other.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
        {
            if (networkObject.OwnerClientId == _ownerClientId) {return;}
        }
        if (other.TryGetComponent<Health>(out Health health))
        {
            health.TakeDamage(_damge);
        }
    }
}
