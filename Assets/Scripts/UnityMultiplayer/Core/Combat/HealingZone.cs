
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class HealingZone : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Image _healPowerBar;
    
    [Header("Settings")]
    [SerializeField] private int _maxHealPower = 30;
    [SerializeField] private float _healCooldown = 60f;
    [SerializeField] private float _healTickRate = 1f; 
    [SerializeField] private int _coinsPerTick = 10;
    [SerializeField] private int _healthPerTick = 10;

    private List<TankPlayer> _playersInZone = new List<TankPlayer>();
    private NetworkVariable<int> _healPower = new NetworkVariable<int>();

    private float _remainingCooldown;
    private float _tickTimer;

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            _healPower.Value = _maxHealPower;
        }

        if (IsClient)
        {
            _healPower.OnValueChanged += HandleHealPowerChanged;
            HandleHealPowerChanged(0, _healPower.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            _healPower.OnValueChanged -= HandleHealPowerChanged;
        }
    }

    private void Update()
    {
        if(!IsServer) return;
        if(_remainingCooldown > 0f)
        {
            _remainingCooldown -= Time.deltaTime;
            if (_remainingCooldown <= 0f)
            {
                _healPower.Value = _maxHealPower;
            }
            else
            {
                return;
            }
        }
        _tickTimer += Time.deltaTime;
        if (_tickTimer >= 1/ _healTickRate)
        {
            foreach(TankPlayer player in _playersInZone)
            {
                if(_healPower.Value == 0) { break; }
                
                if (player.Health.CurrentHealth.Value == player.Health.MaxHealth) { continue; }
                
                if(player.Wallet.TotalCoins.Value < _coinsPerTick) { continue; }

                player.Wallet.SpendCoins(_coinsPerTick);
                player.Health.RestoreHealth(_healthPerTick);
                _healPower.Value --;

                if (_healPower.Value == 0)
                {
                    _remainingCooldown = _healCooldown;
                }
            }
            _tickTimer = _tickTimer % (1 / _healTickRate);
        }
    }
    private void HandleHealPowerChanged(int previousValue, int newValue)
    {
        _healPowerBar.fillAmount = (float)newValue / _maxHealPower;
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(!IsServer) return;
        //We have to use attachedRigidbody because the TankPlayer is actually in the root where the rigid body is and the collider is in a child
        //Is something like a find first In Parent
        if (!other.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer player)){return;} 
        
        _playersInZone.Add(player);
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if(!IsServer) return;
        if (!other.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer player)){return;}

        _playersInZone.Remove(player);  
    }
}
