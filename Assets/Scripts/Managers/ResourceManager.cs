using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

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
                if (!ShouldSkipMissingLogForPath(path))
                {
                    DebugLogger.LogLoadError(path);
                }

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
                if (!ShouldSkipMissingLogForPath(path))
                {
                    DebugLogger.LogLoadError(path);
                }

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

        // path 기반 로그 생략 규칙
        private readonly string[] _skipMissingLogTokens = { "IScene" };

        private bool ShouldSkipMissingLogForPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            // LINQ/Regex 없이 소량 토큰에 대해 O(n) IndexOf 검사
            for (int i = 0; i < _skipMissingLogTokens.Length; i++)
            {
                // Unity .NET 환경 호환을 위해 IndexOf(StringComparison) 사용
                if (path.IndexOf(_skipMissingLogTokens[i], StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}