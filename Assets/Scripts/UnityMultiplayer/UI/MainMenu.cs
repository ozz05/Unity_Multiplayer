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

    private void Start()
    {

        if(ClientSingleton.Instance == null) return;
        //Set the cursor to normal
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        _queueStatusText.text = string.Empty;
        _timeInQueueText.text = string.Empty;    
    }
    public async void FindMatchPressed()
    {
        if(_isCancelling) return;
        if(_isMatchmaking)
        {   
            _queueStatusText.text = "Cancelling...";
            _isCancelling = true;
            //Cancel Matchmaking
            _isCancelling = false;
            _isMatchmaking = false;
            _findMatchButtonText.text = "Find Match";
            _queueStatusText.text = string.Empty;
            return;
        }
        // Start Queue
        _findMatchButtonText.text = "Cancel";
        _queueStatusText.text = "Searching...";
        _isMatchmaking = true;
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
