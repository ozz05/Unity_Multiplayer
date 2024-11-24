using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : MonoBehaviour
{
    [SerializeField] private int _damge = 5;
    [SerializeField] private Projectile _projectile;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.attachedRigidbody == null) return;
        GameObject player = other.attachedRigidbody.gameObject;
        if (_projectile.TeamIndex != -1)
        {
            if (player.TryGetComponent<TankPlayer>(out TankPlayer tankPlayer))
            {
                if (tankPlayer.TeamIndex.Value == _projectile.TeamIndex)
                {
                    return;
                }
            }
        }

        if (player.TryGetComponent<Health>(out Health health))
        {
            health.TakeDamage(_damge);
        }
    }
}
