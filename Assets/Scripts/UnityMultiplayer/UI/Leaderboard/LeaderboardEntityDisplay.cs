using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LeaderboardEntityDisplay : MonoBehaviour
{
    [SerializeField] private  TMP_Text _displayText;
    
    private FixedString32Bytes _displayName;

    public int TeamIndex { get; private set; }
    public ulong ClientId { get; private set; }
    public int Coins { get; private set; }

    public void Initialise(ulong id, FixedString32Bytes displayName, int coins)
    {
        _displayName = displayName;
        ClientId = id; 
        
        UpdateCoins(coins);
    }
    public void Initialise(int teamIndex, FixedString32Bytes displayName, int coins)
    {
        _displayName = displayName;
        TeamIndex = teamIndex;
        
        UpdateCoins(coins);
    }

    public void SetColor(Color color)
    {
        _displayText.color = color;
    }

    public void UpdateCoins(int coins)
    {
        Coins = coins;
        UpdateText();
    }

    public void UpdateText()
    {
        _displayText.text = $"{transform.GetSiblingIndex() + 1}. {_displayName} ({Coins})";
    }
}
