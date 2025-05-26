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
        SetTeam(Team);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcChangeTeam(int newTeam, RpcInfo info = default)
    {
        if (Object.HasStateAuthority)
        {
            Team = newTeam;
        }

        if (PlayerUI != null)
        {
            PlayerUI.UpdateTeamUI(Team);
            SetTeam(newTeam);
        }
    }

    [Rpc]
    public void RpcToggleReady(RpcInfo info = default)
    {
        IsReady = !IsReady;
    }

    private void SetTeam(int team)
    {
        Transform teamPos = team == 0 ? RoomManager.Instance.RedTeam : RoomManager.Instance.BlueTeam;
        transform.SetParent(teamPos, worldPositionStays: false);
    }
}
