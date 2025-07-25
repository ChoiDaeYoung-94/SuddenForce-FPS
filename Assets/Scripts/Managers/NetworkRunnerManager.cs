using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerManager : MonoBehaviour, INetworkRunnerCallbacks
{
    private static NetworkRunnerManager _instance;
    public static NetworkRunnerManager Instance { get { return _instance; } }

    [SerializeField] private NetworkRunner _networkRunner;
    [SerializeField] private NetworkSceneManagerDefault _networkSceneM;

    private List<SessionInfo> _sessionList = new List<SessionInfo>();
    private RoomOptions _roomOptions = new RoomOptions();

    private const string _roomNameMessage = "This room already exists...";

    public string _nickName { get; set; }

    private void Awake()
    {
        _instance = this;

        Init();
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    #region Functions

    private void Init()
    {
        AD.Managers.PopupM.PopupLoading();
        _networkRunner.AddCallbacks(this);

        JoinSessionLobby();
    }

    #region Photon Fusion
    public void RoomSceneSpawn(GameObject prefab, PlayerRef player)
    {
        _networkRunner.SpawnAsync(
            prefab,
            Vector3.zero,
            Quaternion.identity,
            player,
            onBeforeSpawned: (runner, spawnedObj) =>
            {
                Transform teamPosition = RoomManager.Instance.GetTeamPosition();
                int team = teamPosition == RoomManager.Instance.RedTeam ? 0 : 1;
                spawnedObj.transform.SetParent(teamPosition, worldPositionStays: false);
                RoomPlayerNetworkData roomPlayerNetworkData = spawnedObj.GetComponent<RoomPlayerNetworkData>();
                roomPlayerNetworkData.Team = team;
            }
            );
    }

    public void GameSceneSpawn(GameObject prefab, string nickName, int team, PlayerRef player)
    {
        _networkRunner.SpawnAsync(
            prefab,
            Vector3.zero,
            Quaternion.identity,
            player,
            onBeforeSpawned: (runner, spawnedObj) =>
            {
                GamePlayerNetworkData gamePlayerNetworkData = spawnedObj.GetComponent<GamePlayerNetworkData>();
                gamePlayerNetworkData.NickName = nickName;
                gamePlayerNetworkData.Team = team;
                int randomPoint = UnityEngine.Random.Range(0, 4);
                gamePlayerNetworkData.transform.position = team == 0 ?
                SpawnPoints.Instance.RedTeamSpawnPoints[randomPoint].position : SpawnPoints.Instance.BlueTeamSpawnPoints[randomPoint].position;
            }
            );
    }

    public void DeSpawn(NetworkObject player)
    {
        _networkRunner.Despawn(player);
    }

    public async void JoinSessionLobby()
    {
        _networkRunner.ProvideInput = false;

        var result = await _networkRunner.JoinSessionLobby(SessionLobby.ClientServer);

        AD.Managers.PopupM.ClosePopupLoading();

        if (string.IsNullOrEmpty(_nickName))
        {
            AD.Managers.PopupM.PopupSetNickName();
        }

        if (result.Ok)
        {
            AD.DebugLogger.Log("NetworkRunnerM", "JoinSessionLobby successfully.");
        }
        else
        {
            AD.DebugLogger.LogError("NetworkRunnerM", $"Failed to JoinSessionLobby: {result.ShutdownReason}");
        }
    }

    public async void CreateRoom(object value)
    {
        Dictionary<string, object> temp_value = value as Dictionary<string, object>;

        if (_sessionList.Any(s => s.Name == temp_value["RoomName"].ToString()))
        {
            AD.Managers.PopupM.PopupMessage(_roomNameMessage);
            return;
        }

        AD.Managers.PopupM.PopupLoading();

        Dictionary<string, SessionProperty> sessionProperties = new Dictionary<string, SessionProperty>()
        {
            { "MapName", temp_value["MapName"].ToString() }
        };

        var startGameResult = await _networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = temp_value["RoomName"].ToString(),
            PlayerCount = int.Parse(temp_value["MaxPlayers"].ToString()),
            SessionProperties = sessionProperties,
            IsVisible = bool.Parse(temp_value["IsPrivateRoom"].ToString()),
            Scene = SceneRef.FromIndex(2),
            SceneManager = _networkSceneM
        });

        AD.Managers.PopupM.ClosePopupLoading();

        if (startGameResult.Ok)
        {
            _roomOptions.PlayerCount = int.Parse(temp_value["MaxPlayers"].ToString()) / 2;
            _roomOptions.RoomName = temp_value["RoomName"].ToString();
            _roomOptions.MapName = temp_value["MapName"].ToString();
            _roomOptions.Players = $"{_roomOptions.PlayerCount} vs {_roomOptions.PlayerCount}";
            AD.DebugLogger.Log("NetworkRunnerM", $"세션 생성 성공: {temp_value["RoomName"]}");
        }
        else
        {
            AD.DebugLogger.LogError("NetworkRunnerM", $"세션 생성 실패: {startGameResult.ShutdownReason}");
        }
    }

    public async void JoinRoom(SessionInfo sessionInfo)
    {
        AD.Managers.PopupM.PopupLoading();

        AD.DebugLogger.Log("NetworkRunnerM", $"Attempting to join session: {sessionInfo.Name}");

        var joinResult = await _networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = sessionInfo.Name,
            Scene = SceneRef.FromIndex(2),
            SceneManager = _networkSceneM
        });

        AD.Managers.PopupM.ClosePopupLoading();

        if (joinResult.Ok)
        {
            _roomOptions.PlayerCount = sessionInfo.MaxPlayers / 2;
            _roomOptions.RoomName = sessionInfo.Name;
            _roomOptions.MapName = sessionInfo.Properties["MapName"];
            _roomOptions.Players = $"{_roomOptions.PlayerCount} vs {_roomOptions.PlayerCount}";

            AD.DebugLogger.Log("NetworkRunnerM", $"Joined session successfully: {sessionInfo.Name}");
        }
        else
        {
            AD.DebugLogger.LogError("NetworkRunnerM", "Failed to join session: " + joinResult.ShutdownReason);
        }
    }

    public void StartGame()
    {
        _networkRunner.SessionInfo.IsVisible = false;

        int sceneIndex = Enum.GetValues(typeof(AD.GameConstants.Scene)).Length - 1;
        foreach (AD.GameConstants.Scene scene in Enum.GetValues(typeof(AD.GameConstants.GameScene)))
        {
            ++sceneIndex;
            if (_roomOptions.MapName == scene.ToString())
            {
                break;
            }
        }

        RoomManager.Instance.RegisterPlayerInGame();
        RoomManager.Instance.UnregisterAllPlayer();
        AD.Managers.PopupM.PopupSceneLoading();
        AD.Managers.SoundM.PauseBGM();
        _networkRunner.LoadScene(SceneRef.FromIndex(sceneIndex), LoadSceneMode.Additive);
    }
    #endregion

    #region Public Methods
    public void Shutdown()
    {
        AD.Managers.PopupM.PopupLoading();

        _networkRunner.Shutdown();
    }

    public void SaveNickName(string nickName)
    {
        _nickName = nickName;
    }

    public void ChangeMap(string mapName)
    {
        string originalMap = _roomOptions.MapName;

        if (string.Equals(originalMap, mapName) || !_networkRunner.IsServer)
        {
            return;
        }

        var customProps = new Dictionary<string, SessionProperty>()
        {
            ["MapName"] = mapName
        };
        _roomOptions.MapName = mapName;

        if (_networkRunner.SessionInfo.UpdateCustomProperties(customProps))
        {
            RoomManager.Instance.RpcMapChange(_roomOptions.MapName);
            AD.DebugLogger.Log("NetworkRunnerManager", $"{mapName}으로 Map 업데이트 성공");
        }
        else
        {
            _roomOptions.MapName = originalMap;
            AD.DebugLogger.Log("NetworkRunnerManager", $"{mapName}으로 Map 업데이트 실패");
        }
    }

    public void ExitButtonClicked()
    {
        _networkRunner.Shutdown();
    }

    public NetworkRunner GetNetworkRunner()
    {
        return _networkRunner;
    }

    public RoomOptions GetRoomOptions()
    {
        return _roomOptions;
    }
    #endregion

    #region INetworkRunnerCallbacks

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
        {
            return;
        }

        RoomManager.Instance.SpawnRoomPlayer(player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
        {
            return;
        }

        RoomManager.Instance.UnregisterPlayer(player);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (runner.LocalPlayer == PlayerRef.None)
        {
            return;
        }

        CustomPlayerInput data = new CustomPlayerInput();

        var dir = UIManager.Instance.JoyStick.Direction;
        data.MoveX = dir.x;
        data.MoveZ = dir.y;

        if (UIManager.Instance.JoyStick.Magnitude < 5f)
        {
            data.MoveX = 0f;
            data.MoveZ = 0f;
        }

        //data.Fire = JoyStick.Instance.IsPointerDown;

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        AD.DebugLogger.Log("NetworkRunnerM", $"OnShutdown - {shutdownReason}");

        AD.Managers.PopupM.ClosePopupLoading();

        string name = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (name == AD.GameConstants.Scene.Lobby.ToString())
        {

        }
        else if (name == AD.GameConstants.Scene.Room.ToString())
        {
            AD.Managers.SceneM.ChangeScene(AD.GameConstants.Scene.Lobby);
        }
    }

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        AD.DebugLogger.Log("NetworkRunnerM", $"Session list updated. Count: {sessionList.Count}");
        _sessionList = sessionList;

        string name = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (name == AD.GameConstants.Scene.Lobby.ToString())
        {
            RoomManage.Instance.Init(sessionList);
        }
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        string name = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        int sceneCounts = UnityEngine.SceneManagement.SceneManager.sceneCount;

        if (sceneCounts > 1)
        {
            if (name == AD.GameConstants.Scene.Room.ToString())
            {
                AD.Managers.SoundM.UnpauseBGM();
                _networkRunner.ProvideInput = true;

                if (_networkRunner.IsServer)
                {
                    AD.Managers.GameM.Init();
                }
                SceneManager.UnloadSceneAsync(AD.GameConstants.Scene.Room.ToString());
                AD.Managers.PopupM.ClosePopupSceneLoading();
            }
            else if (isGameScene(name))
            {

            }
        }
        else
        {
            if (name == AD.GameConstants.Scene.Room.ToString())
            {
                _roomOptions.IsServer = runner.IsServer;
                CanvasRoom.Instance.Init(_roomOptions);
            }
        }
    }

    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    #endregion

    private bool isGameScene(string sceneName)
    {
        foreach (AD.GameConstants.GameScene gameScene in Enum.GetValues(typeof(AD.GameConstants.GameScene)))
        {
            if (sceneName == gameScene.ToString())
            {
                return true;
            }
        }

        return false;
    }
    #endregion
}

public class RoomOptions
{
    public bool IsServer { get; set; }
    public string RoomName { get; set; }
    public string MapName { get; set; }
    public string Players { get; set; }
    public int PlayerCount { get; set; }
}

public struct CustomPlayerInput : INetworkInput
{
    public float MoveX;
    public float MoveZ;
    public bool Fire;
}