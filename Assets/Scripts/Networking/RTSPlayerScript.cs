using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Mathematics;
using UnityEngine;

public class RTSPlayerScript : NetworkBehaviour
{
     private List<Unit> myUnits = new List<Unit>();
     private List<Building> myBuildings = new List<Building>();
     [SerializeField] private Building[] buildings = new Building[0];
     [SyncVar (hook = nameof(ClientHandleResourcesUpdated))]
     private int resources = 500;

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

     [Server]
     public void SetResources(int newResources)
     {
         resources = newResources;
     }

     #region Server
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

        GameObject buildingInstance = Instantiate(buildingToPlace.gameObject, pos, buildingToPlace.transform.rotation);
        
        NetworkServer.Spawn(buildingInstance,connectionToClient);
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

     public override void OnStopClient()
     {
          if (!isClientOnly || !hasAuthority) return;
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

     #endregion
}
