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
            AD.Debug.Log("Canvas_Main", "Connected and game started successfully.");
        }
        else
        {
            AD.Debug.LogError("Canvas_Main", $"Failed to start the game: {startResult.ShutdownReason}");
        }
    }

    #region Photon Fusion
    private async Task<StartGameResult> ConnectToPhotonFusionServer()
    {
        _runner.ProvideInput = false;
        _runner.AddCallbacks(this);

        var scene = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = null,
            Scene = scene,
            SceneManager = _networkSceneM
        });

        return result;
    }
    #endregion

    #region Public Methods
    public void Shutdown()
    {
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
        AD.Debug.Log("NetworkM", $"OnShutdown - {shutdownReason}");

        AD.Managers.PopupM.ClosePopupLoading();
        AD.Managers.SceneM.NextScene(AD.Define.Scenes.Main);
    }

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
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
