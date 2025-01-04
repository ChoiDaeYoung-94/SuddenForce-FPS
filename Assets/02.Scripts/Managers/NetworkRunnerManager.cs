using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using Fusion;
using Fusion.Sockets;

public class NetworkRunnerManager : MonoBehaviour, INetworkRunnerCallbacks
{
    static NetworkRunnerManager instance;
    public static NetworkRunnerManager Instance { get { return instance; } }

    [Header("--- 세팅 ---")]
    [SerializeField] NetworkRunner _runner = null;
    [SerializeField] NetworkSceneManagerDefault _networkSceneM = null;

    private void Awake()
    {
        instance = this;

        Init();
    }

    private void OnDestroy()
    {
        instance = null;
    }

    #region Functions
    private void Init()
    {
        AD.Managers.PopupM.PopupLoading();
        _runner.AddCallbacks(this);

        JoinSessionLobby();
    }

    #region Photon Fusion
    public async void JoinSessionLobby()
    {
        _runner.ProvideInput = false;

        var result = await _runner.JoinSessionLobby(SessionLobby.ClientServer);

        AD.Managers.PopupM.ClosePopupLoading();

        if (result.Ok)
        {
            AD.Debug.Log("NetworkRunnerM", "JoinSessionLobby successfully.");
        }
        else
        {
            AD.Debug.LogError("NetworkRunnerM", $"Failed to JoinSessionLobby: {result.ShutdownReason}");
        }
    }

    public async void CreateRoom(object value)
    {
        AD.Managers.PopupM.PopupLoading();

        Dictionary<string, object> temp_value = value as Dictionary<string, object>;

        Dictionary<string, SessionProperty> sessionProperties = new Dictionary<string, SessionProperty>()
        {
            { "MapName", temp_value["MapName"].ToString() }
        };

        var startGameResult = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = temp_value["RoomName"].ToString(),
            PlayerCount = int.Parse(temp_value["MaxPlayers"].ToString()),
            SessionProperties = sessionProperties,
            IsVisible = bool.Parse(temp_value["IsPrivateRoom"].ToString()),
            Scene = SceneRef.FromIndex(3),
            SceneManager = _networkSceneM
        });

        AD.Managers.PopupM.ClosePopupLoading();

        if (startGameResult.Ok)
        {
            AD.Debug.Log("NetworkRunnerM", $"세션 생성 성공: {temp_value["RoomName"]}");
        }
        else
        {
            AD.Debug.LogError("NetworkRunnerM", $"세션 생성 실패: {startGameResult.ShutdownReason}");
        }
    }

    public async void JoinRoom(SessionInfo sessionInfo)
    {
        AD.Managers.PopupM.PopupLoading();

        AD.Debug.Log("NetworkRunnerM", $"Attempting to join session: {sessionInfo.Name}");

        var joinResult = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = sessionInfo.Name,
            Scene = SceneRef.FromIndex(3),
            SceneManager = _networkSceneM
        });

        AD.Managers.PopupM.ClosePopupLoading();

        if (joinResult.Ok)
        {
            AD.Debug.Log("NetworkRunnerM", $"Joined session successfully: {sessionInfo.Name}");
        }
        else
        {
            AD.Debug.LogError("NetworkRunnerM", "Failed to join session: " + joinResult.ShutdownReason);
        }
    }
    #endregion

    #region Public Methods
    public void Shutdown()
    {
        AD.Managers.PopupM.PopupLoading();

        _runner.Shutdown();
    }
    #endregion

    #region INetworkRunnerCallbacks
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        AD.Debug.Log("NetworkRunnerM", $"OnShutdown - {shutdownReason}");

        AD.Managers.PopupM.ClosePopupLoading();

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == AD.Define.Scenes.Lobby.ToString())
            AD.Managers.SceneM.NextScene(AD.Define.Scenes.Main);
        else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == AD.Define.Scenes.Room.ToString())
            AD.Managers.SceneM.NextScene(AD.Define.Scenes.Lobby);
    }

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        AD.Debug.Log("NetworkRunnerM", $"Session list updated. Count: {sessionList.Count}");

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
