using UnityEngine;

[CreateAssetMenu(menuName = "AD/Config/PlayFabConfig")]
public class PlayFabConfig : ScriptableObject
{
    public string devTitleId;
    public string stageTitleId;
    public string prodTitleId;

    public string GetTitleId()
    {
#if DEV
        return devTitleId;
#elif STAGE
        return stageTitleId;
#else
        return prodTitleId;
#endif
    }
}