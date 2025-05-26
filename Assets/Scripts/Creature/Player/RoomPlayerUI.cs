using UnityEngine;
using UnityEngine.UI;

public class RoomPlayerUI : MonoBehaviour
{
    public Text NicknameText;
    public Text ReadyStatusText;
    public Image BackgroundImage;

    private int currentTeam;

    public void UpdateTeamUI(int team)
    {
        if (currentTeam != team)
        {
            currentTeam = team;
            //if (team == 1)
            //    BackgroundImage.color = Color.red;
            //else if (team == 2)
            //    BackgroundImage.color = Color.blue;
        }
    }

    public void UpdateUI(string nickname, bool isReady)
    {
        NicknameText.text = nickname;
        ReadyStatusText.text = isReady ? "REAEY" : "";
    }
}
