using System;
using UniRx;
using UnityEngine;

namespace AD
{
    /// <summary>
    /// 애매한 Update() 처리 관리
    /// </summary>
    public class UpdateManager : ISubManager
    {
        public event Action OnUpdateEvent;

        public void Init()
        {
            // UniRx를 활용한 업데이트 이벤트 관리
            Observable.EveryUpdate()
                .Subscribe(_ => OnUpdateEvent?.Invoke())
                .AddTo(Managers.Instance);
        }

        public void release()
        {
            
        }
    }
}
