using System;
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

        public void Init()
        {
            _updateDisposable = Observable.EveryUpdate()
                .Subscribe(_ => OnUpdateEvent?.Invoke());
        }

        public void Release()
        {
            OnUpdateEvent = null;
            _updateDisposable?.Dispose();
            _updateDisposable = null;
        }
    }
}