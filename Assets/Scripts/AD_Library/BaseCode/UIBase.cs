using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AD
{
    public abstract class UIBase : MonoBehaviour
    {
        public enum UIType { Scene, Popup }
        public enum LoadType { StartLoad, DynamicLoad }

        [SerializeField] private UIType _type = UIType.Scene;
        [SerializeField] private LoadType _loadType = LoadType.StartLoad;
        public UIType Type => _type;
        public LoadType LoadMethod => _loadType;

        public bool IsActive => gameObject.activeSelf;
        public bool IsInitialized { get; private set; }
        public bool IsBackButtonDisabled { get; protected set; } = false;   // 닫기 애니메이션/전환 중이거나, 네트워크 요청 중 중복 Back 방지가 필요할 때

        protected virtual void Awake() { }

        public virtual void Activate(params object[] args)
        {
            IsBackButtonDisabled = false;
            gameObject.SetActive(true);
            Init();
        }

        public virtual void Deactivate()
        {
            gameObject.SetActive(false);
            IsBackButtonDisabled = false;
        }

        public async UniTask CloseAsync<T>(Action onClose = null) where T : UIBase
        {
            if (!IsActive)
            {
                return;
            }

            IsBackButtonDisabled = true;
            OnBeforeClose();

            // 닫기 애니메이션 시간 대기 필요 시 여기서 대기
            await UniTask.Yield();

            onClose?.Invoke();
            UIManager.Instance.Deactivate<T>();
        }

        public virtual void Release()
        {
            IsInitialized = false;
        }

        protected virtual void Init()
        {
            IsInitialized = true;
        }

        protected virtual void OnBeforeClose() { }
    }
}