using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using AD.GameData;

namespace AD
{
    public class UIManager : SingletonBase<UIManager>, ISubManager
    {
        private readonly Dictionary<GameConstants.Scene, Dictionary<string, UIBase>> _sceneUIs = new();
        private UIBase _activeSceneUI;
        private Transform _uiRoot;
        private GameConstants.Scene _currentScene;

        public async UniTask InitAsync()
        {
            _uiRoot = transform;
            await UniTask.Yield();
        }

        public void Release()
        {
            ClearAll();
        }

        private void ClearAll()
        {
            foreach (var kv in _sceneUIs)
            {
                foreach (var ui in kv.Value.Values)
                {
                    if (ui != null)
                    {
                        Destroy(ui.gameObject);
                    }
                }
            }
            _sceneUIs.Clear();
            _activeSceneUI = null;

            if (_uiRoot != null)
            {
                for (int i = _uiRoot.childCount - 1; i >= 0; i--)
                {
                    Destroy(_uiRoot.GetChild(i).gameObject);
                }
            }
        }

        /// <summary>
        /// 테이블(UITable) 기반으로 해당 씬의 Scene UI(StartLoad만) 로드
        /// </summary>
        public async UniTask LoadForSceneAsync(GameConstants.Scene scene, Action onLoadCompleted = null)
        {
            _currentScene = scene;

            if (!_sceneUIs.ContainsKey(scene))
            {
                _sceneUIs[scene] = new Dictionary<string, UIBase>();
            }

            // 테이블에서 씬/타입/로드타입 필터
            var rows = Managers.TableManager.FindAll<UITableData>(row =>
                row.Scene == scene &&
                row.Type == UIBase.UIType.Scene &&
                row.LoadType == UIBase.LoadType.StartLoad);

            if (rows == null || rows.Count == 0)
            {
                DebugLogger.LogWarning($"[{nameof(UIManager)}] {scene} 의 Scene UI(StartLoad)가 없습니다.");
                onLoadCompleted?.Invoke();
                await UniTask.Yield();
                return;
            }

            foreach (var r in rows)
            {
                if (_sceneUIs[scene].ContainsKey(r.UIName))
                {
                    continue;
                }

                string path = $"{GameConstants.GetPath(GameConstants.ResourceCategory.UI)}{r.GetKey()}";
                var go = Managers.ResourceManager.InstantiatePrefab(path, _uiRoot);
                
                if (go == null)
                {
                    continue;
                }

                var ui = go.GetComponent<UIBase>();
                if (ui == null)
                {
                    // UIName == 스크립트 타입명 가정
                    Type t = Type.GetType($"AD.{r.UIName}, Assembly-CSharp");
                    if (t == null || !typeof(UIBase).IsAssignableFrom(t))
                    {
                        DebugLogger.LogWarning($"[{nameof(UIManager)}] {r.UIName} 타입을 찾지 못했거나 UIBase가 아닙니다.");
                        continue;
                    }

                    ui = (UIBase)go.GetOrAddComponent(t);
                }
                
                go.SetActive(false);
                _sceneUIs[scene][r.UIName] = ui;
            }

            onLoadCompleted?.Invoke();
            await UniTask.Yield();
        }

        public void UnloadForScene(GameConstants.Scene scene)
        {
            if (!_sceneUIs.TryGetValue(scene, out var dict))
            {
                return;
            }

            foreach (var pair in dict)
            {
                if (pair.Value != null)
                {
                    Destroy(pair.Value.gameObject);
                }
            }
            _sceneUIs.Remove(scene);

            if (_activeSceneUI != null)
            {
                _activeSceneUI = null;
            }
        }

        /// <summary>
        /// 최상단 Scene UI 활성화 (예: Managers.UIManager.Activate<LoginCanvas>())
        /// </summary>
        public T Activate<T>(params object[] args) where T : UIBase
        {
            var ui = FindSceneUI<T>(_currentScene);
            if (ui == null)
            {
                DebugLogger.LogWarning($"[{nameof(UIManager)}] {typeof(T).Name} 를 찾지 못했습니다. (씬: {_currentScene})");
                return null;
            }

            // Scene UI는 뒤(배경)쪽으로 배치 (부모의 자식 목록에서 0번 인덱스(가장 앞)으로 옮김)
            ui.transform.SetAsFirstSibling();
            ui.Activate(args);
            _activeSceneUI = ui;
            return (T)ui;
        }

        public void Deactivate<T>() where T : UIBase
        {
            var ui = FindSceneUI<T>(_currentScene);
            if (ui == null)
            {
                return;
            }

            if (_activeSceneUI == ui)
            {
                _activeSceneUI = null;
            }

            ui.Deactivate();
        }

        private UIBase FindSceneUI<T>(GameConstants.Scene scene) where T : UIBase
        {
            if (!_sceneUIs.TryGetValue(scene, out var dict))
            {
                return null;
            }
            string key = typeof(T).Name; // 프리팹명(UIName)과 타입명이 동일
            return dict.TryGetValue(key, out var ui) ? ui : null;
        }

        // 팝업은 PopupManager에 위임 (필요하면 부모만 UIRoot로 통일)
        public void SetPopupAsChild(Transform popupTransform)
        {
            popupTransform.SetParent(_uiRoot, false);
            popupTransform.SetAsLastSibling(); // 팝업은 항상 최상단
        }
    }
}
