using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class Coin : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] protected int _coinValue;
    protected bool _alreadyCollected;

    public abstract int Collect();

    public void SetValue(int value)
    {
        _coinValue = value;
    }

    protected void Show(bool show)
    {
        _spriteRenderer.enabled = show;
    }
}
