using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawningCoin : Coin
{
    public event Action<RespawningCoin> OnCollected;
    private Vector3 _previousPosition;
    private void Update()
    {
        if (_previousPosition != transform.position)
        {
            if (!IsServer)
                Show(true);
        }
        _previousPosition = transform.position;
    }
    public override int Collect()
    {
        if (!IsServer)
        {
            Show(false);
            return 0;
        }
        
        if (_alreadyCollected) {return 0;}
        _alreadyCollected = true;
        OnCollected?.Invoke(this);
        return _coinValue;
    }

    internal void Reset()
    {
        _alreadyCollected = false;
    }
}
