using UnityEngine;

public class SpawnPoints : MonoBehaviour
{
    private static SpawnPoints _instance;
    public static SpawnPoints Instance { get => _instance; } 

    public Transform[] RedTeamSpawnPoints;
    public Transform[] BlueTeamSpawnPoints;

    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        _instance = null;
    }
}
