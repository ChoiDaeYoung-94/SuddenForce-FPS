using UnityEngine;

/// <summary>
/// 프레임 설정
/// </summary>
public class SetFPS : MonoBehaviour
{
    [SerializeField] private int _fps = 60;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = this._fps;
    }
}
