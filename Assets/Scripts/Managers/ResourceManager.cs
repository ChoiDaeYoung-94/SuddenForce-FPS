using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AD
{
    /// <summary>
    /// Resources 관리
    /// </summary>
    public class ResourceManager : ISubManager
    {
        private readonly Dictionary<string, object> _resourceCache = new();

        public async UniTask InitAsync()
        {
            await UniTask.Yield();
        }

        public void Release()
        {
            _resourceCache.Clear();
            Resources.UnloadUnusedAssets();
        }

        public T Load<T>(string path, bool useCache = false) where T : Object
        {
            if (useCache && _resourceCache.TryGetValue(path, out var cached))
            {
                return cached as T;
            }

            T resource = Resources.Load<T>(path);
            if (resource == null)
            {
                DebugLogger.LogLoadError(path);
                return null;
            }

            if (useCache)
            {
                _resourceCache[path] = resource;
            }

            return resource;
        }

        public async UniTask<T> LoadAsync<T>(string path, bool useCache = false) where T : Object
        {
            if (useCache && _resourceCache.TryGetValue(path, out var cached))
            {
                return cached as T;
            }

            ResourceRequest request = Resources.LoadAsync<T>(path);
            await request.ToUniTask();

            T resource = request.asset as T;
            if (resource == null)
            {
                DebugLogger.LogLoadError(path);
                return null;
            }

            if (useCache)
            {
                _resourceCache[path] = resource;
            }

            return resource;
        }

        public GameObject InstantiatePrefab(string path, Transform parent = null, bool useCache = false)
        {
            GameObject prefab = Load<GameObject>(path, useCache);
            if (prefab == null)
            {
                DebugLogger.LogInstantiateError(path);
                return null;
            }

            return Object.Instantiate(prefab, parent);
        }
    }
}