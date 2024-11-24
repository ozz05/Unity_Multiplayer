using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColorDisplay : MonoBehaviour
{
    [SerializeField] TankPlayer _player;
    [SerializeField] SpriteRenderer _playerSprite;
    [SerializeField] TeamColorLookup _teamColorLookup;
    private void Start()
    {
        _player.TeamIndex.OnValueChanged += HandleTeamIndexChanged;
        HandleTeamIndexChanged(-1, _player.TeamIndex.Value);
    }

    private void HandleTeamIndexChanged(int previousValue, int newValue)
    {
        _playerSprite.color = _teamColorLookup.GetTeamColor(newValue);
    }

    private void OnDestroy()
    {
        _player.TeamIndex.OnValueChanged -= HandleTeamIndexChanged;
    }

    
}
