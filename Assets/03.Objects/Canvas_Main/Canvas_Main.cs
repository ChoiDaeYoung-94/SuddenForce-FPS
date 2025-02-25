using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canvas_Main : MonoBehaviour
{
    #region Functions
    public void GoLobbyScene()
    {
        AD.Managers.SceneM.NextScene(AD.GameConstants.Scene.Lobby);
    }
    #endregion
}
