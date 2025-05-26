using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasLobby : MonoBehaviour
{
    [Header("--- CanvasLobby data ---")]
    [SerializeField] private GameObject _roomsPanel;
    [SerializeField] private GameObject _createRoomPanel;
    [SerializeField] private GameObject _joinPrivateRoomPanel;
    [SerializeField] private TMP_Text _roomNameText;
    [SerializeField] private TMP_Dropdown _maxPlayerDropdown;
    [SerializeField] private Toggle _privateRoomToggle;
    [SerializeField] private TMP_Text _mapNameText;
    [SerializeField] private TMP_Text _privateRoomNameText;

    private int _panelIndex = 0;    // 0 - Rooms, 1 - Create Room, 2 - JoinPrivateRoom

    private const string _roomNameMessage = "Please enter the room name...";
    private const string _mapNameMessage = "Please select a map...";

    #region Functions

    #region Panel SideMenu

    public void Rooms()
    {
        _panelIndex = 0;
        UpdatePanelMain(_panelIndex);
    }

    public void CreateRoom()
    {
        _panelIndex = 1;
        UpdatePanelMain(_panelIndex);
    }

    public void JoinPrivateRoom()
    {
        _panelIndex = 2;
        UpdatePanelMain(_panelIndex);
    }

    public void ExitGame()
    {
        AD.Managers.PopupM.PopupExit();
    }

    #endregion

    #region Panel Main

    private void UpdatePanelMain(int index)
    {
        switch (index)
        {
            case 0:
                PanelRooms();
                break;
            case 1:
                PanelCreateRoom();
                break;
            case 2:
                PanelJoinPrivateRoom();
                break;
            default:
                AD.DebugLogger.LogError("CanvasLobby", "UpdatePanelMain, index error");
                break;
        }
    }

    private void PanelRooms()
    {
        _roomsPanel.SetActive(true);
        _createRoomPanel.SetActive(false);
        _joinPrivateRoomPanel.SetActive(false);
    }

    private void PanelCreateRoom()
    {
        _createRoomPanel.SetActive(true);
        _roomsPanel.SetActive(false);
        _joinPrivateRoomPanel.SetActive(false);
    }

    private void PanelJoinPrivateRoom()
    {
        _joinPrivateRoomPanel.SetActive(true);
        _roomsPanel.SetActive(false);
        _createRoomPanel.SetActive(false);
    }

    #endregion

    #region Rooms

    public void Confirm()
    {
        if (!AD.Utility.IsInvalid(_roomNameText, _roomNameMessage))
        {
            return;
        }

        if (string.IsNullOrEmpty(_mapNameText.text) || _mapNameText.text == "none...")
        {
            AD.Managers.PopupM.PopupMessage(_mapNameMessage);
            return;
        }

        int maxPlayer = int.Parse(_maxPlayerDropdown.options[_maxPlayerDropdown.value].text);

        Dictionary<string, object> roomOption = new Dictionary<string, object>(){
            { "RoomName", _roomNameText.text },
            { "MaxPlayers", maxPlayer },
            { "MapName", _mapNameText.text },
            { "IsPrivateRoom", !_privateRoomToggle.isOn },
        };

        NetworkRunnerManager.Instance.CreateRoom(roomOption);
    }

    public void SelectMap(string mapName)
    {
        _mapNameText.text = mapName;
    }

    public void EnterPrivateRoom()
    {

    }

    #endregion

    public void ClickedUI() => AD.Managers.SoundM.UI_Click();
    public void ClickedOK() => AD.Managers.SoundM.UI_Ok();

    #endregion
}
