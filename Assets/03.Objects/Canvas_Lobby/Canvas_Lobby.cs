using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class Canvas_Lobby : MonoBehaviour
{
    [Header("--- 세팅 ---")]
    [SerializeField] GameObject _go_rooms = null;
    [SerializeField] GameObject _go_createRoom = null;
    [SerializeField] TMP_Text _TMP_roomName = null;
    [SerializeField] TMP_Dropdown _DD_maxPlayer = null;
    [SerializeField] Toggle _tg_privateRoom = null;
    [SerializeField] TMP_Text _TMP_mapName = null;

    [Header("--- 참고용 ---")]
    [SerializeField] int _sideIndex = 0;    // 0 - Rooms, 1 - Create Room

    const string _str_roomName = "Please enter the room name...";
    const string _str_mapName = "Please select a map...";

    #region Functions

    public void GoMainScene()
    {
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
        _go_rooms.SetActive(true);
        _go_createRoom.SetActive(false);
    }

    private void Panel_CreateRoom()
    {
        _go_createRoom.SetActive(true);
        _go_rooms.SetActive(false);
    }
    #endregion

    #region Rooms
    public void Confirm()
    {
        if (string.IsNullOrEmpty(_TMP_roomName.text) || _TMP_roomName.text == "" || _TMP_roomName.text.Length <= 1)
        {
            AD.Managers.PopupM.PopupMessage(_str_roomName);
            return;
        }

        if (string.IsNullOrEmpty(_TMP_mapName.text) || _TMP_mapName.text == "none...")
        {
            AD.Managers.PopupM.PopupMessage(_str_mapName);
            return;
        }

        int maxPlayer = int.Parse(_DD_maxPlayer.options[_DD_maxPlayer.value].text);

        Dictionary<string, object> temp_value = new Dictionary<string, object>(){
            { "RoomName", _TMP_roomName.text },
            { "MaxPlayers", maxPlayer },
            { "MapName", _TMP_mapName.text },
            { "IsPrivateRoom", !_tg_privateRoom.isOn },
        };

        NetworkRunnerManager.Instance.CreateRoom(temp_value);
    }

    public void SelectMap(string mapName)
    {
        _TMP_mapName.text = mapName;
    }
    #endregion

    #endregion
}
