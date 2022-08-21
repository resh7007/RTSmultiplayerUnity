using System;
using Mirror;
using UnityEngine;
using UnityEngine.AI; 

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private float chaseRange = 10f;
    private Camera mainCamera;
    
    #region server

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
       agent.ResetPath();
    }

    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();
        if (targeter.GetTarget() != null)
        {
            if ((target.transform.position-transform.position).sqrMagnitude > chaseRange*chaseRange)
            {
                //chase
                agent.SetDestination(target.transform.position);
            }
            else if (agent.hasPath)
            {
                //stop chasing
                agent.ResetPath();
            }

            return;
        }

        if(!agent.hasPath) return;
        
        if (agent.remainingDistance > agent.stoppingDistance) return;
        
        agent.ResetPath();
    }

    [Command]
    public void CmdMove(Vector3 position)
    {
        targeter.ClearTarget();
        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) return;
        agent.SetDestination(hit.position);
    }
    #endregion

}
