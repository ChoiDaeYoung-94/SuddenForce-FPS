using Fusion;
using UnityEngine;

public class GamePlayerNetworkData : NetworkBehaviour
{
    [SerializeField] private Transform _cameraRoot;
    public Transform CameraRoot => _cameraRoot;

    [Networked] public string NickName { get; set; }
    [Networked] public int Team { get; set; }
    [Networked] public int Health { get; set; }
    [Networked] public int Ammo { get; set; }
    [Networked] public int Kill { get; set; }
    [Networked] public int Death { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Health = 100;
            Ammo = 30;
            Kill = 0;
            Death = 0;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {

    }

    private void OnHealthChanged(int previous, int current)
    {
        if (Object.HasInputAuthority)
        {
            //UIManager.Instance.UpdateHealthBar(current);
        }
    }

    private void OnAmmoChanged(int previous, int current)
    {
        if (Object.HasInputAuthority)
        {
            //UIManager.Instance.UpdateAmmoCount(current);
        }
    }

    public override void FixedUpdateNetwork()
    {

    }
}
