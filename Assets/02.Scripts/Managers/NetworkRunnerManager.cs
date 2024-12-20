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
        Init();
    }

    private void OnDestroy()
    {
        instance = null;
    }

    #region Functions

    private async void Init()
    {
        instance = this;

        AD.Managers.PopupM.PopupLoading();

        var startResult = await ConnectToPhotonFusionServer();

        AD.Managers.PopupM.ClosePopupLoading();

        if (startResult.Ok)
        {
            AD.Debug.Log("NetworkRunnerM", "Connected and game started successfully.");
        }
        else
        {
            AD.Debug.LogError("NetworkRunnerM", $"Failed to start the game: {startResult.ShutdownReason}");
        }
    }

    #region Photon Fusion
    private async Task<StartGameResult> ConnectToPhotonFusionServer()
    {
        _runner.AddCallbacks(this);
        _runner.ProvideInput = false;

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = null,
            Scene = SceneRef.FromIndex(2),
            SceneManager = _networkSceneM
        });

        return result;
    }

    public async void JoinRoom(SessionInfo info)
    {
        AD.Debug.Log("NetworkRunnerM", $"Attempting to join session: {info.Name}");

        var joinResult = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = info.Name,
            Scene = SceneRef.FromIndex(3),
            SceneManager = _networkSceneM
        });

        if (joinResult.Ok)
        {
            AD.Debug.Log("NetworkRunnerM", $"Joined session successfully: {info.Name}");
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

        //룸 목록 정리 후 세션 목록을 최신화하여 UI를 갱신해야 함
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
