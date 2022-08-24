using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private Button startGameButton;
    private void Start()
    {
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
        RTSPlayerScript.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
    }

    private void OnDestroy()
    {
        RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
        RTSPlayerScript.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;

    }

    void HandleClientConnected()
    {
        lobbyUI.SetActive(true);
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
