using System;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [field: SerializeField] public int MaxHealth {get; private set;} = 100;
    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();
    private bool isDeath;

    public Action<Health> onDeath;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;
        CurrentHealth.Value = MaxHealth;
    }

    public void TakeDamage(int damage)
    {
        ModifyHealth(-damage);
    }

    public void RestoreHealth(int healValue)
    {
        ModifyHealth(healValue);
    }

    private void ModifyHealth(int value)
    {
        if (isDeath) return;
        CurrentHealth.Value = Mathf.Clamp(CurrentHealth.Value + value, 0, MaxHealth);
        if (CurrentHealth.Value == 0)
        {
            isDeath = true;
            onDeath?.Invoke(this);
        }
    }
}
