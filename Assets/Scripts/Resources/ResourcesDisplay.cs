using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourcesDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text resourcesText = null;

    private RTSPlayerScript player;

    private void Start()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayerScript>();
       
            ClientHandleResourcesUpdate(player.GetResources());
            player.ClientOnResourcesUpdated += ClientHandleResourcesUpdate;
        
    }
       
   
    private void OnDestroy()
    {
        player.ClientOnResourcesUpdated -= ClientHandleResourcesUpdate;

    }

    private void ClientHandleResourcesUpdate(int resources)
    {
        resourcesText.text = $"Resources: {resources}";
    }
}
