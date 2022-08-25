using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[4];
    private void Start()
    {
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
        RTSPlayerScript.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
        RTSPlayerScript.ClientOnInfoUpdated += ClientHandleOnInfoUpdated;
    }

    private void OnDestroy()
    {
        RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
        RTSPlayerScript.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;
        RTSPlayerScript.ClientOnInfoUpdated -= ClientHandleOnInfoUpdated;

    }

    void HandleClientConnected()
    {
        lobbyUI.SetActive(true);
    }

    void ClientHandleOnInfoUpdated()
    {
        List<RTSPlayerScript> players = ((RTSNetworkManager) NetworkManager.singleton).Players;
        for (int i = 0; i < players.Count; i++)
        {
            playerNameTexts[i].text = players[i].GetDisplayName();
        }
        for (int i =  players.Count; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting For Player...";
        }

        startGameButton.interactable = players.Count >= 2;
    }

    public void LeaveLobby()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
            SceneManager.LoadScene(0);
        }
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool state)
    {
        startGameButton.gameObject.SetActive(state);
    }

    public void StartGame()
    {
        NetworkClient.connection.identity.GetComponent<RTSPlayerScript>().CmdStartGame();
    }
}
