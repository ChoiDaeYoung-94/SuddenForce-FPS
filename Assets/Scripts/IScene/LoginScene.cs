using AD;
using Cysharp.Threading.Tasks;

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