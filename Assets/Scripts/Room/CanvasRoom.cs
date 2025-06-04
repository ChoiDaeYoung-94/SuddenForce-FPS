using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasRoom : NetworkBehaviour
{
    private static CanvasRoom _instance;
    public static CanvasRoom Instance { get { return _instance; } }

    [SerializeField] private RectTransform _teamPanel;
    [SerializeField] private RectTransform _redTeam;
    [SerializeField] private RectTransform _blueTeam;
    [SerializeField] private GridLayoutGroup __redTeamGridLayoutGroup;
    [SerializeField] private GridLayoutGroup __blueTeamGridLayoutGroup;
    [SerializeField] private TMP_Text _roomNameText;
    [SerializeField] private TMP_Text _mapNameText;
    [SerializeField] private TMP_Text _playerCountText;
    [SerializeField] private GameObject _mapChangeButton;
    [SerializeField] private GameObject _mapPanel;
    [SerializeField] private GameObject _readyButton;
    [SerializeField] private Button _startButton;

    public ChatManager _chatManager;

    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    private void Start()
    {
        SetUIResolution();
    }

    public void Init(RoomOptions roomOptions)
    {
        SetRoomOptions(roomOptions);
    }

    private void SetUIResolution()
    {
        float width = _teamPanel.rect.width / 2f;
        float height = (_teamPanel.rect.height - 100f) / 4f;

        _redTeam.sizeDelta = new Vector2(width, _redTeam.sizeDelta.y);
        _blueTeam.sizeDelta = new Vector2(width, _blueTeam.sizeDelta.y);

        __redTeamGridLayoutGroup.cellSize = new Vector2(width, height);
        __blueTeamGridLayoutGroup.cellSize = new Vector2(width, height);
    }

    private void SetRoomOptions(RoomOptions roomOptions)
    {
        _roomNameText.text = roomOptions.RoomName;
        _mapNameText.text = roomOptions.MapName;
        _playerCountText.text = roomOptions.Players;

        if (roomOptions.IsServer)
        {
            _readyButton.SetActive(false);
        }
        else
        {
            _startButton.gameObject.SetActive(false);
            _mapChangeButton.SetActive(false);
        }
    }

    public void ChangeMapName(string mapName)
    {
        _mapNameText.text = mapName;
    }

    #region UI button click
    public void MapChangeButton()
    {
        _mapPanel.SetActive(!_mapPanel.activeSelf);
    }

    public void ChangeMap(string mapName)
    {
        NetworkRunnerManager.Instance.ChangeMap(mapName);
    }

    public void OnTeamSwitchButtonClicked(int team)
    {
        RoomManager.Instance.OnTeamSwitchButtonClicked(team);
    }

    public void ReadyButtonClick()
    {
        RoomManager.Instance.OnReadyButtonClicked();
    }

    public void StartButtonClick()
    {
        if (RoomManager.Instance.IsReady())
        {
            _startButton.enabled = false;
            RoomManager.Instance.OnStartButtonClicked();
        }
    }

    public void ExitButtonClick()
    {
        NetworkRunnerManager.Instance.ExitButtonClicked();
    }
    #endregion

    #region ChatManager
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcBroadcastChat(string message, PlayerRef sender, string nickName, RpcInfo info = default)
    {
        if (NetworkRunnerManager.Instance.GetLocalPlayer() == sender)
        {
            return;
        }

        _chatManager.AddMessage($"<{nickName}> {message}");
    }
    #endregion
}
