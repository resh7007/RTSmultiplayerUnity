using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Targeter : NetworkBehaviour
{
   private Targetable target;

   public Targetable GetTarget()
   {
     return target;
   }

   public override void OnStartServer()
   {
     GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
   }

   public override void OnStopServer()
   {
     GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;

   }

   [Server]
   private void ServerHandleGameOver()
   {
      ClearTarget();
   }

   [Command]
  public void CmdSetTarget(GameObject targetGO)
  {
    if(!targetGO.TryGetComponent<Targetable>(out Targetable _target)) return;
    target = _target;
  }
  
  [Server]
  public void ClearTarget()
  {
    target = null;
  }
 
}
