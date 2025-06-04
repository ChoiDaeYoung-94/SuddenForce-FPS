using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager : NetworkBehaviour
{
    private static RoomManager _instance;
    public static RoomManager Instance { get { return _instance; } }

    [SerializeField] private GameObject _roomPlayer;
    public Transform RedTeam;
    public Transform BlueTeam;

    public List<RoomPlayerNetworkData> RoomPlayers = new List<RoomPlayerNetworkData>();
    public RoomPlayerNetworkData LocalPlayerData;

    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    public void SpawnRoomPlayer(PlayerRef player)
    {
        NetworkRunnerManager.Instance.Spawn(_roomPlayer, player);
    }

    /// <summary>
    /// 플레이어가 Spawn될 때 호출되어 리스트에 추가하고, 로컬 플레이어인 경우 별도로 저장
    /// </summary>
    public void RegisterPlayer(RoomPlayerNetworkData player)
    {
        if (!RoomPlayers.Contains(player))
        {
            RoomPlayers.Add(player);
        }

        if (player.Object.HasInputAuthority)
        {
            LocalPlayerData = player;
        }
    }

    public void UnregisterPlayer(PlayerRef player)
    {
        RoomPlayerNetworkData roomPlayer = RoomPlayers.FirstOrDefault(p => p.Object.InputAuthority == player);
        NetworkRunnerManager.Instance.DeSpawn(roomPlayer.Object);
        RoomPlayers.Remove(roomPlayer);

        RpcBroadcastRemovePlayer(roomPlayer);
    }

    public void UnregisterAllPlayer()
    {
        foreach (RoomPlayerNetworkData player in RoomPlayers)
        {
            NetworkRunnerManager.Instance.DeSpawn(player.Object);
        }
    }

    public void OnReadyButtonClicked()
    {
        if (LocalPlayerData != null && LocalPlayerData.Object.HasInputAuthority)
        {
            LocalPlayerData.RpcRequestToggleReady();
        }
    }

    public void OnStartButtonClicked()
    {
        if (LocalPlayerData != null && LocalPlayerData.Object.HasStateAuthority)
        {
            StartGame();
        }
    }

    public bool IsReady()
    {
        int readyCount = 0;

        foreach (RoomPlayerNetworkData player in RoomPlayers)
        {
            if (player.IsReady)
            {
                ++readyCount;
            }
        }

        if (readyCount == RoomPlayers.Count - 1 && readyCount != 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void OnTeamSwitchButtonClicked(int teamId)
    {
        int teamCount = NetworkRunnerManager.Instance.GetRoomOptions().PlayerCount;
        (int, int) result = GetTeamCount();
        int red = result.Item1, blue = result.Item2;

        if ((teamId == 0 && teamCount == red) || (teamId == 1 && teamCount == blue))
        {
            return;
        }

        if (LocalPlayerData != null && LocalPlayerData.Object.HasInputAuthority)
        {
            LocalPlayerData.RpcChangeTeam(teamId);
        }
    }

    public Transform GetTeamPosition()
    {
        (int, int) result = GetTeamCount();
        int red = result.Item1;

        return NetworkRunnerManager.Instance.GetRoomOptions().PlayerCount > red ? RedTeam : BlueTeam;
    }

    private (int, int) GetTeamCount()
    {
        int red = 0, blue = 0;

        foreach (RoomPlayerNetworkData player in RoomPlayers)
        {
            if (player.Team == 0)
            {
                ++red;
            }
            else
            {
                ++blue;
            }
        }

        return (red, blue);
    }

    private void StartGame()
    {
        NetworkRunnerManager.Instance.StartGame();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcMapChange(string mapName, RpcInfo info = default)
    {
        CanvasRoom.Instance.ChangeMapName(mapName);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcBroadcastRemovePlayer(RoomPlayerNetworkData player, RpcInfo info = default)
    {
        RoomPlayers.Remove(player);
    }
}
