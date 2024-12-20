using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canvas_Lobby : MonoBehaviour
{
    [Header("--- 세팅 ---")]
    [SerializeField] GameObject _go_Rooms = null;
    [SerializeField] GameObject _go_CreateRoom = null;

    [Header("--- 참고용 ---")]
    [SerializeField] int _sideIndex = 0;    // 0 - Rooms, 1 - Create Room

    #region Functions

    public void GoMainScene()
    {
        AD.Managers.PopupM.PopupLoading();

        NetworkRunnerManager.Instance.Shutdown();
    }

    #region Panel_SideMenu
    public void Rooms()
    {
        _sideIndex = 0;
        UpdatePanel_Main(_sideIndex);
    }

    public void CreateRoom()
    {
        _sideIndex = 1;
        UpdatePanel_Main(_sideIndex);
    }
    #endregion

    #region Panel_Main
    private void UpdatePanel_Main(int index)
    {
        switch (index)
        {
            case 0:
                Panel_Room();
                break;
            case 1:
                Panel_CreateRoom();
                break;
            default:
                AD.Debug.LogError("Canvas_Lobby", "UpdatePanel_Main, index error");
                break;
        }
    }

    private void Panel_Room()
    {
        _go_Rooms.SetActive(true);
        _go_CreateRoom.SetActive(false);
    }

    private void Panel_CreateRoom()
    {
        _go_CreateRoom.SetActive(true);
        _go_Rooms.SetActive(false);
    }
    #endregion

    #endregion
}
