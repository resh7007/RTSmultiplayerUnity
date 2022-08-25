using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class RTSNetworkManager : NetworkManager
{

  [SerializeField] private GameObject unitBasePrefab=null;
  [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

  public static event Action ClientOnConnected;
  public static event Action ClientOnDisconnected;
  public List<RTSPlayerScript> Players { get; } = new List<RTSPlayerScript>() ;
  private bool isGameInProgress = false;

  #region Server

  public override void OnServerConnect(NetworkConnectionToClient conn)
  {
      if(!isGameInProgress) return;
      conn.Disconnect();
      
  }

  public override void OnServerDisconnect(NetworkConnectionToClient conn)
  {
      RTSPlayerScript player = conn.identity.GetComponent<RTSPlayerScript>();

      Players.Remove(player);
      
      base.OnServerDisconnect(conn);
  }

  public override void OnStopServer()
  {
      Players.Clear();
      isGameInProgress = false;
  }

  public void StartGame()
  {
      if(Players.Count < 2) return;
      isGameInProgress = true;
      
      ServerChangeScene("map1");
  }

  public override void OnServerAddPlayer(NetworkConnectionToClient conn)
  {
      base.OnServerAddPlayer(conn);

      RTSPlayerScript player = conn.identity.GetComponent<RTSPlayerScript>();
      Players.Add(player);
      
      player.SetDisplayName($"Player {Players.Count}");
      player.SetColor(new Color(
          Random.Range(0f,1f),
          Random.Range(0f,1f),
          Random.Range(0f,1f)));
      
      player.SetPartyOwner(Players.Count==1);
  
  }

  public override void OnServerSceneChanged(string sceneName)
  {
      if (SceneManager.GetActiveScene().name.StartsWith("map1"))
      {
          GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
           
          NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

          foreach (RTSPlayerScript player in Players)
          {
             GameObject baseInstance = Instantiate(unitBasePrefab, GetStartPosition().position, Quaternion.identity);
             
             NetworkServer.Spawn(baseInstance, player.connectionToClient);
          }
      }
  }

  #endregion

  #region Client

  public override void OnClientConnect(NetworkConnection conn)
  {
      base.OnClientConnect(conn);
      ClientOnConnected?.Invoke();
  }

  public override void OnClientDisconnect(NetworkConnection conn)
  {
      base.OnClientDisconnect(conn);
      ClientOnDisconnected?.Invoke();

  }

  public override void OnStopClient()
  {
      Players.Clear();
  }

  #endregion


}
