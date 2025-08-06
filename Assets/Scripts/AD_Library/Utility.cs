using MiniJSON;
using System;
using TMPro;
using UnityEngine;

namespace AD
{
    /// <summary>
    /// 유틸리티 기능 제공
    /// </summary>
    public static class Utility
    {
        #region JSON handling

        /// <summary>
        /// 객체를 JSON 문자열로 변환
        /// </summary>
        public static string SerializeToJson(object obj)
        {
            return Json.Serialize(obj);
        }

        /// <summary>
        /// JSON 문자열을 객체로 변환
        /// </summary>
        public static object DeserializeFromJson(string jsonData)
        {
            return Json.Deserialize(jsonData);
        }

        #endregion

        #region Component handling

        /// <summary>
        /// GameObject에서 특정 컴포넌트를 가져오거나, 없으면 추가 후 반환
        /// </summary>
        public static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            if (gameObject == null)
            {
                DebugLogger.LogError("GetOrAddComponent: gameObject is null.");
                return null;
            }

            T component = gameObject.GetComponent<T>();
            return component ?? gameObject.AddComponent<T>();
        }

        #endregion

        public static bool IsInvalid(TMP_Text text, string message)
        {
            if (string.IsNullOrEmpty(text.text) || text.text == "" || text.text.Length <= 1 ||
                text.text.Replace(" ", "").Length == 1)
            {
                //Managers.PopupManager.PopupMessage(message);
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    #region Singleton

    /// <summary>
    /// Singleton이 manager가 아니라 따로 설정 시 사용
    /// 예로 -> [SingletonPrefabPath("UI/CanvasRoom")]를 class위에 붙여 사용
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SingletonPrefabPathAttribute : Attribute
    {
        public string Path { get; }

        public SingletonPrefabPathAttribute(string path)
        {
            Path = path;
        }
    }

    public class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
    {
        static private T _instance;

        static public T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindFirstObjectByType<T>();
                    if (_instance == null)
                    {
                        string typeString = typeof(T).Name;
                        string path = GetPrefabPathFromAttribute();

                        GameObject prefab = Managers.ResourceManager.Load<GameObject>(path);
                        if (prefab != null)
                        {
                            GameObject go = GameObject.Instantiate(prefab);
                            go.name = typeString;
                            _instance = go.GetComponent<T>() ?? go.AddComponent<T>();
                        }
                        else
                        {
                            GameObject go = new GameObject(typeString);
                            _instance = go.AddComponent<T>();
                        }

                        if (Application.isPlaying)
                            DontDestroyOnLoad(_instance.gameObject);
                    }
                }

                return _instance;
            }
        }

        private static string GetPrefabPathFromAttribute()
        {
            var attr = typeof(T).GetCustomAttributes(typeof(SingletonPrefabPathAttribute), false);
            if (attr.Length > 0 && attr[0] is SingletonPrefabPathAttribute pathAttr)
            {
                return pathAttr.Path;
            }

            return $"Managers/{typeof(T).Name}";
        }
    }

    #endregion

    #region UI

    // public abstract class UIBase : MonoBehaviour
    // {
    //     public enum UIType
    //     {
    //         Scene,  // 하나의 UI만 존재
    //         Popup   // 씬 전환에도 유지되거나, 빈 영역 클릭 시 닫힘
    //     }
    //
    //     public enum LoadType
    //     {
    //         StartLoad,     // 시작 시 로드됨
    //         DynamicLoad    // 동적으로 로드됨
    //     }
    //
    //     [SerializeField] private UIType _type = UIType.Scene;
    //     [SerializeField] private LoadType _loadType = LoadType.StartLoad;
    //
    //     public UIType Type => _type;
    //     public LoadType LoadMethod => _loadType;
    //     public bool IsActive => gameObject.activeSelf;
    //     public bool IsInitialized { get; private set; }
    //     public bool IsBackButtonDisabled { get; protected set; } = false;
    //
    //     /// <summary> 연출/페이드 완료 여부 </summary>
    //     public bool ActivateComplete { get; protected set; } = false;
    //
    //     protected virtual void Awake() { }
    //
    //     public virtual void Activate(params object[] args)
    //     {
    //         IsBackButtonDisabled = false;
    //         gameObject.SetActive(true);
    //         Init();
    //     }
    //
    //     public virtual void Deactivate()
    //     {
    //         gameObject.SetActive(false);
    //         IsBackButtonDisabled = false;
    //     }
    //
    //     public async UniTask CloseAsync<T>(Action onClose = null) where T : UIBase
    //     {
    //         if (!IsActive) return;
    //
    //         IsBackButtonDisabled = true;
    //         OnBeforeClose();
    //
    //         // 닫기 애니메이션 시간만큼 대기 필요 시
    //         await UniTask.Yield(); // 또는 await UniTask.Delay(x);
    //
    //         onClose?.Invoke();
    //         UIManager.Instance.Deactivate<T>();
    //     }
    //
    //     public virtual void OnClickBack()
    //     {
    //         // 필요 시 상속받은 UI에서 구현
    //     }
    //
    //     public virtual void Release()
    //     {
    //         IsInitialized = false;
    //     }
    //
    //     protected virtual void Init()
    //     {
    //         IsInitialized = true;
    //     }
    //
    //     protected virtual void OnBeforeClose()
    //     {
    //         // 닫기 애니메이션 or 효과 준비 시
    //     }
    //
    //     /// <summary>
    //     /// 화면 탭 이벤트 (필요 시 오버라이드)
    //     /// </summary>
    //     public virtual void OnTabScreen(Transform selectedTransform, Vector3 worldPos) { }
    // }

    #endregion
}