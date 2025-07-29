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
            if (string.IsNullOrEmpty(text.text) || text.text == "" || text.text.Length <= 1 || text.text.Replace(" ","").Length == 1)
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
}
