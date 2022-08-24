using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Mathematics;
using UnityEngine;

public class RTSPlayerScript : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private Building[] buildings = new Building[0];
    [SerializeField] private float buildingRangeLimit = 5;

    private Color teamColor = new Color();
    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();
    [SyncVar (hook = nameof(ClientHandleResourcesUpdated))]
    private int resources = 500;
    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool isPartyOwner = false;

    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;
    public bool GetIsPartyOwner()
    {
        return isPartyOwner;
    }

    public Transform GetCameraTransform()
    {
        return cameraTransform;
    }

    public Color GetTeamColor()
    {
        return teamColor;
    }

    public event Action<int> ClientOnResourcesUpdated;

     public int GetResources()
     {
         return resources;
     }

     public List<Unit> GetMyUnit()
     {
        return myUnits;
     }
     public List<Building> GetMyBuildings()
     {
        return myBuildings;
     }



     public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 pos)
     {
         if (Physics.CheckBox(pos + buildingCollider.center,
                 buildingCollider.size / 2,
                 Quaternion.identity,
                 buildingBlockLayer))
         {
             return false;
         }
  
         foreach (Building building in myBuildings)
         { 
             if ((pos - building.transform.position).sqrMagnitude <= buildingRangeLimit * buildingRangeLimit)
             {
                 return true;
             }
         }

         return false;

     }

     #region Server

     [Server]
     public void SetPartyOwner(bool state)
     {
         isPartyOwner = state;
     }

     [Server]
     public void SetResources(int newResources)
     {
         resources = newResources;
     }

     [Server]
     public void SetColor(Color newColor)
     { 
         teamColor = newColor;
     }
     public override void OnStartServer()
     {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawn;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawn;
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;

     }

     public override void OnStopServer()
     {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawn;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawn;
        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;

     }

     [Command]
     public void CmdStartGame()
     {
         if(!isPartyOwner) return;
         
         ((RTSNetworkManager)NetworkManager.singleton).StartGame();
         
     }

     [Command]
     public void CmdTryPlaceBuilding(int buildingId, Vector3 pos)
     {
         Building buildingToPlace = null;
         foreach (Building building in buildings)
         {
             if (building.GetId() == buildingId)
             {
                 buildingToPlace = building;
                 break;
             }
         }

         if (buildingToPlace == null) return;
        
         if(resources < buildingToPlace.GetPrice()) return;

         BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();
            
         if(!CanPlaceBuilding(buildingCollider, pos)) return;
        
         GameObject buildingInstance = Instantiate(buildingToPlace.gameObject, pos, buildingToPlace.transform.rotation);
        
         NetworkServer.Spawn(buildingInstance,connectionToClient);
         
         SetResources(resources - buildingToPlace.GetPrice());
     }

     private void ServerHandleBuildingSpawned(Building building)
     {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;
        
        myBuildings.Add(building);
     }
     private void ServerHandleBuildingDespawned(Building building)
     {
          if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;
          myBuildings.Remove(building);

     }
     
     private void ServerHandleUnitSpawn(Unit unit)
     {
          if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;
          
          myUnits.Add(unit);
     }
     private void ServerHandleUnitDespawn(Unit unit)
     {
          if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;
          myUnits.Remove(unit);

     }
     #endregion

     #region Client

     public override void OnStartAuthority()
     {
          if (NetworkServer.active) return; // if this machine is running as a server then return
          Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawn;
          Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawn;
          Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawn;
          Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawn;

     }

     public override void OnStartClient()
     {
         if (NetworkServer.active) return; // if this machine is running as a server then return
           ((RTSNetworkManager) NetworkManager.singleton).Players.Add(this);
     }

     public override void OnStopClient()
     {
          if (!isClientOnly) return;
          ((RTSNetworkManager) NetworkManager.singleton).Players.Remove(this);
          if (!hasAuthority) return;

          Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawn;
          Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawn;
          
          Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawn;
          Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawn;
     }

     private void AuthorityHandleUnitSpawn(Unit unit)
     {
         myUnits.Add(unit);
     }
     private void AuthorityHandleUnitDespawn(Unit unit)
     {
     
         myUnits.Remove(unit);
     }
     
     private void AuthorityHandleBuildingSpawn(Building building)
     {
         myBuildings.Add(building);
     }
     private void AuthorityHandleBuildingDespawn(Building building)
     {
     
         myBuildings.Remove(building);
     }

     private void ClientHandleResourcesUpdated(int oldResources, int newResources)
     {
         ClientOnResourcesUpdated?.Invoke(newResources);
     }

     private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState)
     {
         if(!hasAuthority) return;

         AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
     }

     #endregion
}
