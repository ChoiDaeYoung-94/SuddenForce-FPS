using System;
using UnityEngine;

/// <summary>
/// 초기화 순서 지정
/// </summary>
public class InitializeLobby : MonoBehaviour
{
    /// <summary>
    /// 초기화 해야하는 스크립트들의 이름을 그대로 선언
    /// -> 먼저 적은 순으로 초기화 진행
    /// </summary>
    private enum Scripts
    {

    }

    [Tooltip("초기화 해야 할 스크립트를 지닌 게임오브젝트")]
    [SerializeField] private GameObject[] _initialzeObjects = null;

    private void Awake()
    {
        foreach (Scripts script in Enum.GetValues(typeof(Scripts)))
        {
            foreach (GameObject item in _initialzeObjects)
            {
                if (item.GetComponent(script.ToString()) != null)
                {
                    // 실행해야 할 메서드
                    item.GetComponent(script.ToString()).SendMessage("");
                    break;
                }
            }
        }

        //AD.Managers.CreateNetworkRunner();
    }
}
