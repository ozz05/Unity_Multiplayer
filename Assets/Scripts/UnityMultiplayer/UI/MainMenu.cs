using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text _queueStatusText;
    [SerializeField] private TMP_Text _timeInQueueText;
    [SerializeField] private TMP_Text _findMatchButtonText;
    [SerializeField] private TMP_InputField _joinCodeField;

    private bool _isMatchmaking;
    private bool _isCancelling;
    private bool _isBusy;
    private float _timeInQueue;

    private void Start()
    {

        if(ClientSingleton.Instance == null) return;
        //Set the cursor to normal
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        _queueStatusText.text = string.Empty;
        _timeInQueueText.text = string.Empty;    
    }

    private void Update()
    {
        if(_isMatchmaking)
        {
            _timeInQueue += Time.deltaTime;
            UpdateTimeInQueueText();
        }
    }

    private void UpdateTimeInQueueText()
    {
        // Convert the float to TimeSpan
        TimeSpan time = TimeSpan.FromSeconds(_timeInQueue);

        // Format the TimeSpan as minutes and seconds
        string formattedTime = String.Format("{0:00}:{1:00}", time.Minutes, time.Seconds);
        _timeInQueueText.text = formattedTime;

    }
    public async void FindMatchPressed()
    {
        if(_isCancelling) return;

        if(_isMatchmaking)
        {   
            _queueStatusText.text = "Cancelling...";
            _isCancelling = true;
            //Cancel Matchmaking
            await ClientSingleton.Instance.GameManager.CancelMatchmaking();
            _isCancelling = false;
            _isMatchmaking = false;
            _findMatchButtonText.text = "Find Match";
            _queueStatusText.text = string.Empty;
            _timeInQueueText.text = String.Empty;
            _isBusy = false;
            return;
        }

        if (_isBusy) return;
        _isBusy = true;
        // Start Queue
        ClientSingleton.Instance.GameManager.MatchmakeAsync(OnMatchMade);
        _findMatchButtonText.text = "Cancel";
        _queueStatusText.text = "Searching...";
        _isMatchmaking = true;
        _timeInQueue = 0;
        UpdateTimeInQueueText();
    }

    private void OnMatchMade(MatchmakerPollingResult result)
    {
        switch(result)
        {
            case MatchmakerPollingResult.Success:
                _queueStatusText.text = "Connecting...";
                break;
            default:
                _queueStatusText.text = result.ToString();
                break;
        }
    }
    public async void StartHost()
    {
        if (_isBusy) return;
        _isBusy = true;
        await HostSingleton.Instance.GameManager.StartHostAsync();
        _isBusy = false;
    }

    public async void StartClient()
    {
        if (_isBusy) return;
        _isBusy = true;
        await ClientSingleton.Instance.GameManager.StartClientAsync(_joinCodeField.text);
        _isBusy = false;
    }

    public async void JoinAsync(Lobby lobby)
    {
        if (_isBusy) return;
        _isBusy = true;
        try
        {
            Lobby joinnigLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
            string joinCode = joinnigLobby.Data["JoinCode"].Value;

            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex);
        }
        _isBusy = false;
    }
}
