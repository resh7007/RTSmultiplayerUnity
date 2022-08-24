using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class RTSNetworkManager : NetworkManager
{

  [SerializeField] private GameObject unitSpawnerPrefab=null;
  [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

  public static event Action ClientOnConnected;
  public static event Action ClientOnDisconnected;


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

  public override void OnServerAddPlayer(NetworkConnectionToClient conn)
   {
      base.OnServerAddPlayer(conn);

      RTSPlayerScript player = conn.identity.GetComponent<RTSPlayerScript>();
      player.SetColor(new Color(
          Random.Range(0f,1f),
          Random.Range(0f,1f),
          Random.Range(0f,1f)));
      
      // GameObject unitSpawnerInstance= Instantiate(unitSpawnerPrefab, conn.identity.transform.position,
      //     conn.identity.transform.rotation);
      //
      // NetworkServer.Spawn(unitSpawnerInstance, conn);
   }

   public override void OnServerSceneChanged(string sceneName)
   {
       if (SceneManager.GetActiveScene().name.StartsWith("map1"))
       {
           GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
           
           NetworkServer.Spawn(gameOverHandlerInstance.gameObject);
       }
   }
}
