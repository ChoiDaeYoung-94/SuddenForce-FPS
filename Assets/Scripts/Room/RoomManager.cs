using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : NetworkBehaviour
{
    private static RoomManager _instance;
    public static RoomManager Instance { get { return _instance; } }

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

    /// <summary>
    /// 플레이어가 퇴장할 때 호출
    /// </summary>
    public void UnregisterPlayer(RoomPlayerNetworkData player)
    {
        RoomPlayers.Remove(player);
    }

    public void OnReadyButtonClicked()
    {
        if (LocalPlayerData != null && LocalPlayerData.Object.HasInputAuthority)
        {
            LocalPlayerData.RpcToggleReady();
        }
    }

    public void OnTeamSwitchButtonClicked(int teamId)
    {
        if (LocalPlayerData != null && LocalPlayerData.Object.HasInputAuthority)
        {
            LocalPlayerData.RpcChangeTeam(teamId);
        }
    }

    public void CheckTeamReadyStatus()
    {
        int redReadyCount = 0;
        int blueReadyCount = 0;

        foreach (RoomPlayerNetworkData player in RoomPlayers)
        {
            if (player.Team == 1 && player.IsReady)
                redReadyCount++;
            else if (player.Team == 2 && player.IsReady)
                blueReadyCount++;
        }

        if (redReadyCount >= 1 && blueReadyCount >= 1)
        {
            StartGame();
        }
    }

    private void StartGame()
    {

    }
}
