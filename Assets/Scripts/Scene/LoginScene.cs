using AD;
using Cysharp.Threading.Tasks;

[SingletonPrefabPath("Empty")]
public class LoginScene : SingletonBase<LoginScene>, IScene
{
    /// <summary>
    /// 작업
    /// - 사운드 호출
    /// - UIManager에서 해당 씬 관련 UI 로드 후 action으로 받아서 UIManager를 통해 UI 활성화
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public UniTask InitAsync()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// 작업
    /// - UIManager에서 해당 씬 관련 UI 언로드
    /// - 오브젝트 Destroy
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public UniTask ReleaseAsync()
    {
        throw new System.NotImplementedException();
    }
}
