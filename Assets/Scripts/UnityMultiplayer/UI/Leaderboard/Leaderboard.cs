using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Unity.Netcode;
using UnityEngine;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField] private Transform _leaderboardEntityHolder;
    [SerializeField] private LeaderboardEntityDisplay _leaderboardEntityPrefab;

    private NetworkList<LeaderboardEntityState> _leaderboardEntities;
    private List<LeaderboardEntityDisplay> _entityDisplays = new List<LeaderboardEntityDisplay>();
    [SerializeField] private int _entitiesToDisplay = 8;

    private void Awake()
    {
        _leaderboardEntities = new NetworkList<LeaderboardEntityState>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            _leaderboardEntities.OnListChanged += HandleLeaderboardEntitiesChanged;
            foreach (LeaderboardEntityState entry in _leaderboardEntities)
            {
                HandleLeaderboardEntitiesChanged(new NetworkListEvent<LeaderboardEntityState> 
                {
                    Type = NetworkListEvent<LeaderboardEntityState>.EventType.Add,
                    Value = entry
                });
            }
        }
        if (IsServer)
        {
            TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
            foreach (TankPlayer player in players)
            {
                HandlePlayerSpawn(player);
            }
            TankPlayer.OnPlayerSpawned += HandlePlayerSpawn;
            TankPlayer.OnPlayerDespawned += HandlePlayerDespawn;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            TankPlayer.OnPlayerSpawned -= HandlePlayerSpawn;
            TankPlayer.OnPlayerDespawned -= HandlePlayerDespawn;
        }

        if (IsClient)
        {
            _leaderboardEntities.OnListChanged -= HandleLeaderboardEntitiesChanged;
        }
    }
    private void CreateLeaderboardItem(LeaderboardEntityState value)
    {
        //This is basically a for loop that checks for any values inside that has the same ClientId
        if (_entityDisplays.Any(x => x.ClientId == value.ClientId)) return;

        LeaderboardEntityDisplay display = Instantiate(_leaderboardEntityPrefab, _leaderboardEntityHolder);
        display.Initialise(value.ClientId, value.PlayerName, value.Coins);
        _entityDisplays.Add(display);
    }
    private void HandleLeaderboardEntitiesChanged(NetworkListEvent<LeaderboardEntityState> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<LeaderboardEntityState>.EventType.Add:
                CreateLeaderboardItem(changeEvent.Value);
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:
                LeaderboardEntityDisplay displayToRemove =
                    _entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if (displayToRemove != null)
                {
                    displayToRemove.transform.SetParent(null);
                    _entityDisplays.Remove(displayToRemove);
                    Destroy(displayToRemove);
                }
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Value:
                LeaderboardEntityDisplay displayToUpdate =
                    _entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if (displayToUpdate != null)
                {
                    displayToUpdate.UpdateCoins(changeEvent.Value.Coins);
                }
                    break;
        }
        //this compares the things in the _entitiesDisplay from greater than to smaller if you want the opposite just switch x and y
        _entityDisplays.Sort((x, y) => y.Coins.CompareTo(x.Coins));
        for (int i = 0; i < _entityDisplays.Count; i++)
        {
            //This sets the order on witch they are in the hierarchy 
            _entityDisplays[i].transform.SetSiblingIndex(i);
            // updates the text values
            _entityDisplays[i].UpdateText();
            _entityDisplays[i].gameObject.SetActive(i < _entitiesToDisplay);
        }
        //Gets the players _entityDisplay
        LeaderboardEntityDisplay myDisplay  = _entityDisplays.FirstOrDefault(x => x.ClientId == NetworkManager.Singleton.LocalClientId);

        if (myDisplay != null)
        {
            //Check if the players is not showing in the leaderboard
            if (myDisplay.transform.GetSiblingIndex() >= _entitiesToDisplay)
            {
                //Disable the last member that shows in the leaderboar
                _leaderboardEntityHolder.GetChild(_entitiesToDisplay -1).gameObject.SetActive(false);
                //Display ourselfs
                myDisplay.gameObject.SetActive(true);

            }
        }
    }

    private void HandlePlayerSpawn(TankPlayer player)
    {
        _leaderboardEntities.Add(new LeaderboardEntityState
        {
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            Coins = 0
        });

        player.Wallet.TotalCoins.OnValueChanged += (oldCoins, newCoins) =>
            HandleCoinsValueChanged(player.OwnerClientId, newCoins);
    }

    private void HandlePlayerDespawn(TankPlayer player)
    {
        if (_leaderboardEntities == null) { return; }

        foreach (var entity in _leaderboardEntities)
        {
            if(entity.ClientId != player.OwnerClientId)
            {
                continue;
            }
            _leaderboardEntities.Remove(entity);
            break;
        }

        player.Wallet.TotalCoins.OnValueChanged -= (oldCoins, newCoins) =>
            HandleCoinsValueChanged(player.OwnerClientId, newCoins);
    }

    private void HandleCoinsValueChanged(ulong clientId, int newCoins)
    {
        for (int i = 0; i < _leaderboardEntities.Count; i++)
        {
            if (_leaderboardEntities[i].ClientId != clientId) {continue;}
            _leaderboardEntities[i] = new LeaderboardEntityState
            {
                ClientId = clientId,
                PlayerName = _leaderboardEntities[i].PlayerName.Value,
                Coins = newCoins
            };
            return;
        }
    }
}
