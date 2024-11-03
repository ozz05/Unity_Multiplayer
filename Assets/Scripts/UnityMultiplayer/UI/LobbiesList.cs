using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;


public class LobbiesList : MonoBehaviour
{
    [SerializeField] private LobbyItem _lobbyItemPrefab;
    [SerializeField] private Transform _lobbyItemParent;
    
    private bool _isJoining;
    private bool _isRefreshing;

    private void OnEnable()
    {
        RefreshList();
    }
    public async void RefreshList()
    {
        if (_isRefreshing) return;
        _isRefreshing = true;

        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;
            //Display Filter
            options.Filters = new List<QueryFilter>()
            {
                //Makes sure that it only show available lobbies
                new QueryFilter(
                    field : QueryFilter.FieldOptions.AvailableSlots,
                    op : QueryFilter.OpOptions.GT,
                    value: "0"
                    ),
                // Makes sure that the lobby is not locked
                new QueryFilter(
                    field : QueryFilter.FieldOptions.IsLocked,
                    op : QueryFilter.OpOptions.EQ,
                    value: "0"
                    )
            };

            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
            foreach (Transform child in _lobbyItemParent)
            {
                Destroy(child.gameObject);
            }

            foreach(Lobby lobby in lobbies.Results)
            {
                LobbyItem lobbyItem = Instantiate(_lobbyItemPrefab, _lobbyItemParent);
                lobbyItem.Initialise(this, lobby);
            }

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }

        

        _isRefreshing = false;

    }
    public async void JoinAsync(Lobby lobby)
    {
        if (_isJoining) return;
        _isJoining = true;
        try
        {
            Lobby joinnigLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
            string joinCode = joinnigLobby.Data["JoinCode"].Value;

            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        _isJoining = false;
    }
}
