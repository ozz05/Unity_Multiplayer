
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;

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

    private List<TankPlayer> _playersInZone;
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(!IsServer) return;
        //We have to use attachedRigidbody because the TankPlayer is actually in the root where the rigid body is and the collider is in a child
        //Is something like a find first In Parent
        if (!other.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer player)){return;} 
        
        if (!_playersInZone.Contains(player))
        {
            _playersInZone.Add(player);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if(!IsServer) return;
        if (!other.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer player)){return;}

        _playersInZone.Remove(player);  
    }
}
