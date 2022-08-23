using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class TeamColorSetter : NetworkBehaviour
{
    [SerializeField] private Renderer[] colorRenderers = new Renderer[0];

    [SyncVar (hook = nameof(HandleTeamColorUpdated))] private Color teamColor = new Color();

    #region Server

    public override void OnStartServer()
    {
        RTSPlayerScript player = connectionToClient.identity.GetComponent<RTSPlayerScript>();
        teamColor = player.GetTeamColor();
    }

    #endregion

    #region Client

    private void HandleTeamColorUpdated(Color oldColor, Color newColor)
    {
        foreach (Renderer renderer in colorRenderers)
        {
            renderer.materials[0].SetColor("_BaseColor", newColor);//change
            if(renderer.materials.Length>1)
                renderer.materials[1].SetColor("_BaseColor", newColor);//change

        }
    }

    #endregion
}
