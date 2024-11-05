using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class PlayerNameDisplay : MonoBehaviour
{
    [SerializeField] private TankPlayer _player;
    [SerializeField] private TMP_Text _playerNameText;
    private void Start()
    {
        HandlePlayerNameChanged(string.Empty, _player.PlayerName.Value);
        _player.PlayerName.OnValueChanged += HandlePlayerNameChanged;
    }

    private void HandlePlayerNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        _playerNameText.text = newName.ToString();
    }

    private void OnDestroy()
    {
        _player.PlayerName.OnValueChanged -= HandlePlayerNameChanged;
    }
}
