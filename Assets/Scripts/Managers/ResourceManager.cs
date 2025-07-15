using UnityEngine;

namespace AD
{
    /// <summary>
    /// Resources 관리
    /// </summary>
    public class ResourceManager : ISubManager
    {
        public void Init()
        {
            
        }

        public void release()
        {
            
        }

        public T Load<T>(string path) where T : Object
        {
            T resource = Resources.Load<T>(path);
            if (resource == null)
                DebugLogger.LogLoadError(path);

            return resource;
        }

        public GameObject InstantiatePrefab(string path, Transform parent = null)
        {
            GameObject prefab = Load<GameObject>("Prefabs/" + path);
            if (prefab == null)
            {
                DebugLogger.LogInstantiateError(path);
                return null;
            }

            return Object.Instantiate(prefab, parent);
        }
    }
}