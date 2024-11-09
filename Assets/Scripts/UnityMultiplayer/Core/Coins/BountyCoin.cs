using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BountyCoin : Coin
{
    public override int Collect()
    {
        if (!IsServer)
        {
            Show(false);
            return 0;
        }
        
        if (_alreadyCollected) {return 0;}
        _alreadyCollected = true;
        Destroy(gameObject);
        return _coinValue;
    }
}
