using System;
using AD;
using UnityEngine;

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