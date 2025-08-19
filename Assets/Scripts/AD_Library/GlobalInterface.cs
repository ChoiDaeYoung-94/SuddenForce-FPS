using Cysharp.Threading.Tasks;

namespace AD
{
    public interface ISubManager
    {
        UniTask InitAsync();
        void Release();
    }
    
    public interface IScene
    {
        UniTask InitAsync();
        UniTask ReleaseAsync();
    }
    
    public interface ITableData
    {
        string GetKey();
    }
}