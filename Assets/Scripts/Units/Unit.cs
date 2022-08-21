using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour
{
    [SerializeField] private int resourceCost = 10;
    [SerializeField] private Health health = null;
    [SerializeField] private UnitMovement _unitMovement = null;
    [SerializeField] private UnityEvent onSelected = null;
    [SerializeField] private UnityEvent onDeSelected = null;
    [SerializeField] private Targeter targeter = null;
    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawned;
    
    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;

    public int GetResourceCost()
    {
        return resourceCost;
    }

    public Targeter GetTargeter()
    {
        return targeter;
    }
    public UnitMovement GetUnitMovement()
    {
        return _unitMovement;
    }


    #region  Server

    public override void OnStartServer()
    {
        base.OnStartServer();
        ServerOnUnitSpawned?.Invoke(this);
        health.ServerOnDie += ServerHandleDie; 

    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        ServerOnUnitDespawned?.Invoke(this);
        health.ServerOnDie -= ServerHandleDie; 

    }
    

    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }
    #endregion
    #region Client
    [Client]
    public void Select()
    {
        if (!hasAuthority) return;
        onSelected.Invoke();
    }
    [Client]
    public void Deselect()
    {
        if (!hasAuthority) return;

        onDeSelected.Invoke();
    }

    public override void OnStartClient()
    {
        AuthorityOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
  
        if(!hasAuthority) return;

        AuthorityOnUnitDespawned?.Invoke(this);

    }

    #endregion
}
