using UnityEngine;

using Fusion;

public class RoomPlayerNetworkData : NetworkBehaviour
{
    [Networked] public string NickName { get; set; }
    [Networked] public bool IsReady { get; set; }
    [Networked] public int Team { get; set; }

    public RoomPlayerUI PlayerUI { get; set; }

    public override void Spawned()
    {
        RoomManager.Instance.RegisterPlayer(this);
    }

    private void OnTeamChanged()
    {
        // UI가 할당되어 있으면 변경된 팀 값을 UI에 반영
        if (PlayerUI != null)
        {
            PlayerUI.UpdateTeamUI(Team);
        }
    }

    [Rpc]
    public void RpcChangeTeam(int newTeam, RpcInfo info = default)
    {
        Team = newTeam;
    }

    [Rpc]
    public void RpcToggleReady(RpcInfo info = default)
    {
        IsReady = !IsReady;
    }
}
