using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] private RectTransform unitSelectonArea = null;
    [SerializeField] private LayerMask _layerMask = new LayerMask();
    private Camera mainCamera;

    private Vector2 startPosition;
    private RTSPlayerScript player;
 
    public List<Unit> SelectedUnits { get;  } = new List<Unit>();

    private void Start()
    {
        mainCamera = Camera.main;
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawn;
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
        player = NetworkClient.connection.identity.GetComponent<RTSPlayerScript>();

    }

    private void OnDestroy()
    {
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawn;
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;

    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
        else if (Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }
    }
  
    private void StartSelectionArea()
    {
        if(!Keyboard.current.leftShiftKey.isPressed)
        {
            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Deselect();
            }
            SelectedUnits.Clear();
        }
        
        unitSelectonArea.gameObject.SetActive(true);
        startPosition = Mouse.current.position.ReadValue();
        UpdateSelectionArea();
    }

    private void UpdateSelectionArea()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        float areaWidth = mousePosition.x - startPosition.x;
        float areaHeight = mousePosition.y - startPosition.y;
        unitSelectonArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectonArea.anchoredPosition = startPosition + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    private void ClearSelectionArea()
    {
        unitSelectonArea.gameObject.SetActive(false);
        
        if(unitSelectonArea.sizeDelta.magnitude==0)
        {
            //for single unit selection only
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _layerMask))
                 return;
            if (!hit.collider.TryGetComponent<Unit>(out Unit unit)) return;

            if (!unit.hasAuthority) return;
            SelectedUnits.Add(unit);

            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Select();
            }

            return;
        }

        Vector2 min = unitSelectonArea.anchoredPosition - (unitSelectonArea.sizeDelta / 2);
        Vector2 max = unitSelectonArea.anchoredPosition + (unitSelectonArea.sizeDelta / 2);

        foreach (Unit unit in player.GetMyUnit())
        {
            if(SelectedUnits.Contains(unit)) continue;
            
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);

            if (screenPosition.x > min.x && screenPosition.x < max.x &&
                screenPosition.y > min.y && screenPosition.y < max.y)
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }

    }
    private void AuthorityHandleUnitDespawn(Unit unit)
    {
        SelectedUnits.Remove(unit);
    }

    private void ClientHandleGameOver(string winner)
    {
        enabled = false;
    }

}
