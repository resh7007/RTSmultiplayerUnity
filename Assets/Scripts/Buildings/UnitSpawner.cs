using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Health health = null;
    [SerializeField] private GameObject unitPrefab = null;
    [SerializeField] private Transform unitSpawnPoint = null;
    #region server

    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;

    }

    [Server]
    private void ServerHandleDie()
    {
         NetworkServer.Destroy(gameObject);
    }

    [Command]
    private void CmdSpawnUnity()
    {
        GameObject unitInstance = Instantiate(unitPrefab, unitSpawnPoint.position, unitSpawnPoint.rotation);
        
        NetworkServer.Spawn(unitInstance, connectionToClient);
    }
    #endregion
    
    #region client
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return; // if  right mouse btn was pressed 
        
        if(!hasAuthority) return; //if i pressed myself not enemy
        CmdSpawnUnity();
    }

    #endregion

 
}
