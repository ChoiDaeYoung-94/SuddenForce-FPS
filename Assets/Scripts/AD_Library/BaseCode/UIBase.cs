using System;
using AD;
using Cysharp.Threading.Tasks;
using UnityEngine;

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
    public bool IsBackButtonDisabled { get; protected set; } = false;
    public bool ActivateComplete { get; protected set; } = false;

    protected virtual void Awake() { }

    public virtual void Activate(params object[] args)
    {
        IsBackButtonDisabled = false;
        gameObject.SetActive(true);
        Init();
        ActivateComplete = true;
    }

    public virtual void Deactivate()
    {
        gameObject.SetActive(false);
        IsBackButtonDisabled = false;
        ActivateComplete = false;
    }

    public async UniTask CloseAsync<T>(Action onClose = null) where T : UIBase
    {
        if (!IsActive) return;

        IsBackButtonDisabled = true;
        OnBeforeClose();

        // 닫기 애니메이션 시간 대기 필요 시 여기서 대기
        await UniTask.Yield();

        onClose?.Invoke();
        UIManager.Instance.Deactivate<T>();
    }

    public virtual void OnClickBack() { /* 필요 시 오버라이드 */ }

    public virtual void Release()
    {
        IsInitialized = false;
    }

    protected virtual void Init()
    {
        IsInitialized = true;
    }

    protected virtual void OnBeforeClose() { }

    public virtual void OnTabScreen(Transform selectedTransform, Vector3 worldPos) { }
}