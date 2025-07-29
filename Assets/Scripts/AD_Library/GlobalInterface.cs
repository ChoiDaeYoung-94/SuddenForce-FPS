using Cysharp.Threading.Tasks;

namespace AD
{
    public interface IScene
    {
        UniTask InitAsync();
        UniTask ReleaseAsync();
    }
}