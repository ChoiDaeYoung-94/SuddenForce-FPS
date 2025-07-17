using System.Collections.Generic;
using UnityEngine;

namespace AD
{
    /// <summary>
    /// 풀 관리 클래스
    /// 다양한 GameObject/UI 풀을 생성 및 관리하며 재사용성을 높임
    /// </summary>
    public class PoolManager : SingletonBase<PoolManager>, ISubManager
    {
        private enum PoolType
        {
            GameObject,
            UI
        }

        #region Nested Pool Class

        private class Pool
        {
            public Transform Root;
            private GameObject TargetPrefab;
            private Stack<PoolObject> _poolStack = new();

            public void Init(GameObject prefab, int count)
            {
                TargetPrefab = prefab;

                for (int i = 0; i < count; i++)
                {
                    PoolObject poolObj = CreatePoolObject();
                    PushToPool(poolObj);
                }
            }

            private PoolObject CreatePoolObject()
            {
                GameObject newObj = Object.Instantiate(TargetPrefab);
                newObj.name = TargetPrefab.name;
                PoolObject poolObj = newObj.GetOrAddComponent<PoolObject>();
                return poolObj;
            }

            public void PushToPool(PoolObject poolObj)
            {
                poolObj.transform.SetParent(Root);
                poolObj.gameObject.SetActive(false);
                _poolStack.Push(poolObj);
            }

            public GameObject PopFromPool(Transform parent)
            {
                PoolObject poolObj = _poolStack.Count > 0 ? _poolStack.Pop() : CreatePoolObject();
                poolObj.gameObject.SetActive(true);
                poolObj.transform.SetParent(parent);
                return poolObj.gameObject;
            }
        }

        #endregion

        private readonly Dictionary<string, Dictionary<string, Pool>> _scenePoolMap = new();

        public void Init()
        {
            
        }

        public void Release()
        {
            if (_scenePoolMap == null)
            {
                return;
            }

            var sceneKeys = new List<string>(_scenePoolMap.Keys);
            foreach (var sceneName in sceneKeys)
            {
                ClearScenePools(sceneName);
            }

            _scenePoolMap.Clear();
        }

        public void ClearScenePools(string sceneName)
        {
            if (_scenePoolMap.TryGetValue(sceneName, out var poolDict))
            {
                var keys = new List<string>(poolDict.Keys);

                foreach (var poolKey in keys)
                {
                    Pool pool = poolDict[poolKey];

                    if (pool.Root != null)
                        Object.Destroy(pool.Root.gameObject);

                    poolDict.Remove(poolKey);
                }

                _scenePoolMap.Remove(sceneName);
            }
        }

        #region Pool root

        private Transform GetOrCreateSceneRoot(string sceneName)
        {
            Transform sceneRoot = gameObject.transform.Find(sceneName)?.transform;
            if (sceneRoot == null)
            {
                GameObject go = new GameObject(sceneName);
                go.transform.SetParent(gameObject.transform);
                sceneRoot = go.transform;
            }

            return sceneRoot;
        }

        private Transform GetOrCreateSceneCategoryRoot(string sceneName, PoolType type)
        {
            Transform sceneRoot = GetOrCreateSceneRoot(sceneName);
            string categoryName = type == PoolType.GameObject ? "GameObject" : "UI";

            Transform categoryRoot = sceneRoot.Find(categoryName);
            if (categoryRoot == null)
            {
                GameObject go = new GameObject(categoryName);
                categoryRoot = go.transform;
                categoryRoot.SetParent(sceneRoot);
            }

            return categoryRoot;
        }

        private Transform CreatePoolRoot(string sceneName, PoolType type, string poolName)
        {
            Transform categoryRoot = GetOrCreateSceneCategoryRoot(sceneName, type);
            GameObject poolRootGO = new(poolName);
            Transform poolRoot = poolRootGO.transform;
            poolRoot.SetParent(categoryRoot);
            return poolRoot;
        }

        #endregion

        #region Pool Functions

        public void CreatePool(string sceneName, GameObject prefab, bool isGameObjectPool, int count = 20)
        {
            if (prefab == null)
            {
                DebugLogger.LogError("Prefab is null while creating pool.");
                return;
            }

            if (!_scenePoolMap.ContainsKey(sceneName))
            {
                _scenePoolMap[sceneName] = new Dictionary<string, Pool>();
            }

            var poolDict = _scenePoolMap[sceneName];
            if (poolDict.ContainsKey(prefab.name))
            {
                DebugLogger.LogWarning($"Pool for {prefab.name} already exists in {sceneName}.");
                return;
            }

            Pool pool = new()
            {
                Root = CreatePoolRoot(sceneName, isGameObjectPool ? PoolType.GameObject : PoolType.UI, prefab.name)
            };
            pool.Init(prefab, count);

            poolDict[prefab.name] = pool;
        }

        public void PushToPool(string sceneName, GameObject go)
        {
            if (go == null) return;

            PoolObject poolObj = go.GetComponent<PoolObject>();
            if (poolObj == null)
            {
                Object.Destroy(go);
                return;
            }

            if (!_scenePoolMap.TryGetValue(sceneName, out var poolDict) || !poolDict.TryGetValue(go.name, out var pool))
            {
                Object.Destroy(go);
                return;
            }

            pool.PushToPool(poolObj);
        }

        public GameObject PopFromPool(string sceneName, string prefabName, Transform parent = null)
        {
            if (!_scenePoolMap.TryGetValue(sceneName, out var poolDict) ||
                !poolDict.TryGetValue(prefabName, out var pool))
            {
                DebugLogger.LogNotFound($"Pool not found: {sceneName}/{prefabName}");
                return null;
            }

            return pool.PopFromPool(parent);
        }

        #endregion
    }
}