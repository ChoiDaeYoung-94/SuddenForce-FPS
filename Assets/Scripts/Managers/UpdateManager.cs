using System;
using Cysharp.Threading.Tasks;
using UniRx;

namespace AD
{
    /// <summary>
    /// 애매한 Update() 처리 관리
    /// </summary>
    public class UpdateManager : ISubManager
    {
        public event Action OnUpdateEvent;
        private IDisposable _updateDisposable;

        public async UniTask InitAsync()
        {
            _updateDisposable = Observable.EveryUpdate()
                .Subscribe(_ => OnUpdateEvent?.Invoke());
            
            await UniTask.Yield();
        }

        public void Release()
        {
            OnUpdateEvent = null;
            _updateDisposable?.Dispose();
            _updateDisposable = null;
        }
    }
}