using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LeaderboardEntityDisplay : MonoBehaviour
{
    [SerializeField] private  TMP_Text _displayText;
    [SerializeField] private Color _myColor;
    
    private FixedString32Bytes _playerName;

    public ulong ClientId { get; private set; }
    public int Coins { get; private set; }

    public void Initialise(ulong id, FixedString32Bytes playerName, int coins)
    {
        _playerName = playerName;
        ClientId = id; 
        if (ClientId == NetworkManager.Singleton.LocalClientId)
        {
            _displayText.color = _myColor;
        }
        UpdateCoins(coins);
    }

    public void UpdateCoins(int coins)
    {
        Coins = coins;
        UpdateText();
    }

    public void UpdateText()
    {
        _displayText.text = $"{transform.GetSiblingIndex() + 1}. {_playerName} ({Coins})";
    }
}
