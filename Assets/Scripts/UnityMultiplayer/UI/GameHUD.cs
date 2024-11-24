using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameHUD : NetworkBehaviour
{
    [SerializeField] private TMP_Text _lobbyCodeText;
    NetworkVariable<FixedString32Bytes> _lobbyCode = new NetworkVariable<FixedString32Bytes>("");

    public void LeaveGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.GameManager.Shutdown();
        }

        ClientSingleton.Instance.GameManager.Disconnect();
    }

    public override void OnNetworkSpawn()
    {
        if(IsClient)
        {
            _lobbyCode.OnValueChanged += HandleLobbyCodeChanged;
            HandleLobbyCodeChanged(string.Empty, _lobbyCode.Value);
        }

        if (!IsHost) return;
        _lobbyCode.Value = HostSingleton.Instance.GameManager.JoinCode;
        
        
    }
    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            _lobbyCode.OnValueChanged -= HandleLobbyCodeChanged;
        }
    }
    private void HandleLobbyCodeChanged(FixedString32Bytes oldCode, FixedString32Bytes newCode)
    {
        _lobbyCodeText.text = newCode.ToString();
    }
}
