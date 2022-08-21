using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour
{
    public static event Action ServerOnGameOver;
    public static event Action<string> ClientOnGameOver;

    private List<UnitBase> bases = new List<UnitBase>();
    #region Server

    public override void OnStartServer()
    {
        UnitBase.ServerOnBaseSpawn += ServerHandleBaseSpawn;
        UnitBase.ServerOnBaseDespawn += ServerHandleBaseDespawn;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnBaseSpawn -= ServerHandleBaseSpawn;
        UnitBase.ServerOnBaseDespawn -= ServerHandleBaseDespawn;

    }

    [Server]
    private void ServerHandleBaseSpawn(UnitBase unitbase)
    {
        bases.Add(unitbase);
        
    }
    [Server]
    private void ServerHandleBaseDespawn(UnitBase unitbase)
    {
        bases.Remove(unitbase);
        
        if(bases.Count!=1) return;

        int playerId = bases[0].connectionToClient.connectionId;
       RpcGameOver($"Player {playerId}");
       
       ServerOnGameOver?.Invoke();
    }
    #endregion
    #region Client

    [ClientRpc]
    private void RpcGameOver(string winner)
    {
        ClientOnGameOver?.Invoke(winner);
    }

    #endregion
}
