using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayerUI : MonoBehaviour
{

    [SerializeField] private TMP_Text _nickName;
    [SerializeField] private Image _teamImage;
    [SerializeField] private GameObject _readyStateObject;

    public void UpdateTeamUI(int team)
    {
        _teamImage.color = team == 0 ? new Color(1f, 0f, 0f, 0.5f) : new Color(0f, 0f, 1f, 0.5f);
    }

    public void SetNickName(string nickName)
    {
        _nickName.text = nickName;
    }

    public void SetReadyState(bool active)
    {
        _readyStateObject.SetActive(active);
    }
}
