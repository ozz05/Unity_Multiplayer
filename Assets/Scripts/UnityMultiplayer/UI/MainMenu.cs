using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField _joinCodeField;
    [SerializeField] private TMP_Text _queueStatusText;
    [SerializeField] private TMP_Text _timeInQueueText;
    [SerializeField] private TMP_Text _findMatchButtonText;
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
        if (_isMatchmaking)
        {
            _timeInQueue += Time.deltaTime;
            TimeSpan ts = TimeSpan.FromSeconds(_timeInQueue);
            _timeInQueueText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
        }
    }

    public async void FindMatchPressed()
    {
        if (_isCancelling) { return; }

        if (_isMatchmaking)
        {
            _queueStatusText.text = "Cancelling...";
            _isCancelling = true;
            await ClientSingleton.Instance.GameManager.CancelMatchmaking();
            _isCancelling = false;
            _isMatchmaking = false;
            _isBusy = false;
            _findMatchButtonText.text = "Find Match";
            _queueStatusText.text = string.Empty;
            _timeInQueueText.text = string.Empty;
            return;
        }

        if (_isBusy) { return; }
        //Start match
        ClientSingleton.Instance.GameManager.MatchmakeAsync(OnMatchMade);
        _findMatchButtonText.text = "Cancel";
        _queueStatusText.text = "Searching...";
        _timeInQueue = 0f;
        _isMatchmaking = true;
        _isBusy = true;

    }

    private void OnMatchMade(MatchmakerPollingResult result)
    {
        switch (result)
        {
            case MatchmakerPollingResult.Success:
                _queueStatusText.text = "Connecting...";
                break;
            default:
                _queueStatusText.text = $"{result}";
                break;
        }
    }
    public async void StartHost()
    {
        await HostSingleton.Instance.GameManager.StartHostAsync();
    }

    public async void StartClient()
    {
        await ClientSingleton.Instance.GameManager.StartClientAsync(_joinCodeField.text);
    }
}
