using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandGiver : MonoBehaviour
{
    [SerializeField] private UnitSelectionHandler _unitSelectionHandler = null;
    [SerializeField] private LayerMask _layerMask = new LayerMask();
    private Camera mainCamera;
 
    void Start()
    {
        mainCamera = Camera.main;
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;

    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;

    }

    void Update()
    {
        if (!Mouse.current.rightButton.wasPressedThisFrame) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity,_layerMask)) return;
        if (hit.collider.TryGetComponent<Targetable>(out Targetable target))
        {
            if (target.hasAuthority)
            {
                TryMove(hit.point);
                    return;
            }

            TryTarget(target);
            print(target.name);
            return;
        }

        TryMove(hit.point);

    }

    void TryMove(Vector3 point)
    {
        foreach (Unit unit in _unitSelectionHandler.SelectedUnits)
        {
            unit.GetUnitMovement().CmdMove(point);
        }
    }
    
    void TryTarget(Targetable target)
    {
        foreach (Unit unit in _unitSelectionHandler.SelectedUnits)
        {
            unit.GetTargeter().CmdSetTarget(target.gameObject);
        }
    }

    private void ClientHandleGameOver(string winner)
    {
        enabled = false;
    }
}
