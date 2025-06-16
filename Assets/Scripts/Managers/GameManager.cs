using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject[] _gamePlayerObject;
    public List<PlayerRef> Players = new List<PlayerRef>();
    public List<string> NickNames = new List<string>();
    public List<int> Teams = new List<int>();

    public void Init()
    {
        SpawnGamePlayer();
    }

    public void SpawnGamePlayer()
    {
        for (int i = 0; i < Players.Count; i++)
        {
            NetworkRunnerManager.Instance.GameSceneSpawn(_gamePlayerObject[Teams[i]], NickNames[i], Teams[i], Players[i]);
        }
    }
}
