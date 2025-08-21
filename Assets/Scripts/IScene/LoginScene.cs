using AD;
using Cysharp.Threading.Tasks;

/// <summary>
/// Login scene의 경우 Bootstrap이후 AD.Manager를 통해 진입하지 않기 때문에 해당 코드는 예시용
/// </summary>
[SingletonPrefabPath("Empty, IScene")]
public class LoginScene : SingletonBase<LoginScene>, IScene
{
    public async UniTask InitAsync()
    {
        //Managers.SoundManager.PlayBGM(BGMType.LoginTest);
        await Managers.UIManager.LoadForSceneAsync(GameConstants.Scene.Login, OnLoadComplete);
    }

    public async UniTask ReleaseAsync()
    {
        Managers.UIManager.UnloadForScene(GameConstants.Scene.Login);
        await UniTask.Yield();
        Destroy(this);
    }

    private void OnLoadComplete()
    {
        Managers.UIManager.Activate<LoginCanvas>();
    }
}