using AD;
using Cysharp.Threading.Tasks;

public class UIManager : SingletonBase<UIManager>, ISubManager
{
    public async UniTask InitAsync()
    {
        await UniTask.Yield();
    }

    public void Release()
    {
        
    }
}
