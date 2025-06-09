using Fusion;
using UnityEngine;

public class RoomPlayerNetworkData : NetworkBehaviour
{
    [Networked] public string NickName { get; set; }
    [Networked] public bool IsReady { get; set; }
    [Networked] public int Team { get; set; }

    public RoomPlayerUI PlayerUI;

    public override void Spawned()
    {
        RoomManager.Instance.RegisterPlayer(this);

        if (Object.HasInputAuthority)
        {
            RpcSetNickName(NetworkRunnerManager.Instance._nickName);
        }

        SetTeam(Team);
        PlayerUI.UpdateTeamUI(Team);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RpcSetNickName(string nick, RpcInfo info = default)
    {
        if (Object.HasStateAuthority)
        {
            NickName = nick;
        }

        RpcBroadcastSetNickName(info.Source);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcBroadcastSetNickName(PlayerRef who, RpcInfo info = default)
    {
        foreach (RoomPlayerNetworkData player in RoomManager.Instance.RoomPlayers)
        {
            player.PlayerUI.SetNickName(player.NickName);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcChangeTeam(int newTeam, RpcInfo info = default)
    {
        Team = newTeam;
        if (PlayerUI != null)
        {
            PlayerUI.UpdateTeamUI(Team);
            SetTeam(newTeam);
        }
    }

    private void SetTeam(int team)
    {
        Transform teamPos = team == 0 ? RoomManager.Instance.RedTeam : RoomManager.Instance.BlueTeam;
        transform.SetParent(teamPos, worldPositionStays: false);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RpcRequestToggleReady(RpcInfo info = default)
    {
        IsReady = !IsReady;
        PlayerUI.SetReadyState(IsReady);
        RpcBroadcastReady(IsReady, info.Source);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcBroadcastReady(bool newState, PlayerRef who, RpcInfo info = default)
    {
        foreach (RoomPlayerNetworkData player in RoomManager.Instance.RoomPlayers)
        {
            if (player.Object.InputAuthority == who)
            {
                player.PlayerUI.SetReadyState(newState);
            }
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);

        string name = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (name == AD.GameConstants.Scene.Room.ToString())
        {
            if (!Object.HasInputAuthority)
            {
                RoomManager.Instance.RemovePlayer(this);
            }
        }
    }
}
