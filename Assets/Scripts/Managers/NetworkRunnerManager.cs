using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkRunnerManager : MonoBehaviour, INetworkRunnerCallbacks
{
    private static NetworkRunnerManager _instance;
    public static NetworkRunnerManager Instance { get { return _instance; } }

    [Header("--- 세팅 ---")]
    [SerializeField] private NetworkRunner _networkRunner;
    [SerializeField] private NetworkSceneManagerDefault _networkSceneM;

    private List<SessionInfo> _sessionList = new List<SessionInfo>();

    private const string _roomNameMessage = "This room already exists...";

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

    public void Spawn(GameObject prefab, Transform parentTransform, PlayerRef player)
    {
        _networkRunner.SpawnAsync(
            prefab,
            Vector3.zero,
            Quaternion.identity,
            player,
            onBeforeSpawned: (runner, spawnedObj) =>
            {
                spawnedObj.transform.SetParent(parentTransform, worldPositionStays: false);
            }
            );
    }

    public async void JoinSessionLobby()
    {
        _networkRunner.ProvideInput = false;

        var result = await _networkRunner.JoinSessionLobby(SessionLobby.ClientServer);

        AD.Managers.PopupM.ClosePopupLoading();

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
            AD.DebugLogger.Log("NetworkRunnerM", $"세션 생성 성공: {temp_value["RoomName"]}");
        }
        else
        {
            AD.DebugLogger.LogError("NetworkRunnerM", $"세션 생성 실패: {startGameResult.ShutdownReason}");
        }
    }

    public async void JoinRoom(string roomName)
    {
        AD.Managers.PopupM.PopupLoading();

        AD.DebugLogger.Log("NetworkRunnerM", $"Attempting to join session: {roomName}");

        var joinResult = await _networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = roomName,
            Scene = SceneRef.FromIndex(2),
            SceneManager = _networkSceneM
        });

        AD.Managers.PopupM.ClosePopupLoading();

        if (joinResult.Ok)
        {
            _networkRunner.ProvideInput = true;

            AD.DebugLogger.Log("NetworkRunnerM", $"Joined session successfully: {roomName}");
        }
        else
        {
            AD.DebugLogger.LogError("NetworkRunnerM", "Failed to join session: " + joinResult.ShutdownReason);
        }
    }

    #endregion

    #region Public Methods

    public void Shutdown()
    {
        AD.Managers.PopupM.PopupLoading();

        _networkRunner.Shutdown();
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

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        AD.DebugLogger.Log("NetworkRunnerM", $"OnShutdown - {shutdownReason}");

        AD.Managers.PopupM.ClosePopupLoading();

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == AD.GameConstants.Scene.Room.ToString())
            AD.Managers.SceneM.ChangeScene(AD.GameConstants.Scene.Lobby);
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

        RoomManage.Instance.Init(sessionList);
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    #endregion

    #endregion
}
