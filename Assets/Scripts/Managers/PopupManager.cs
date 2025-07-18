using System.Collections.Generic;
using TMPro;
using UnityEditor.EditorTools;
using UnityEngine;

namespace AD
{
    public class PopupManager : SingletonBase<PopupManager>, ISubManager
    {
        private enum CommonPopupType
        {
            PopupMessage,
            PopupLoading,
            PopupSceneLoading,
        }
        
        private GameObject _popupLoading;
        private GameObject _popupSceneLoading;
        private Stack<GameObject> _popupStack = new();
        private bool _isException = true;
        private bool _isFlow = false;

        public void Init()
        {
            Managers.UpdateManager.OnUpdateEvent -= OnUpdate;
            Managers.UpdateManager.OnUpdateEvent += OnUpdate;
            CommonPopupInit();
        }

        public void Release()
        {
            Managers.UpdateManager.OnUpdateEvent -= OnUpdate;
            _popupStack.Clear();
        }

        private void OnUpdate()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (Input.GetKeyDown(KeyCode.Escape) && !_isFlow)
                {
                    DisablePop();
                }
            }
        }

        #region Init

        private void CommonPopupInit()
        {
            string path = GameConstants.GetPath(GameConstants.ResourceCategory.Popup) + CommonPopupType.PopupLoading;
            _popupLoading = Managers.ResourceManager.InstantiatePrefab(path, transform);
            path = GameConstants.GetPath(GameConstants.ResourceCategory.Popup) + CommonPopupType.PopupSceneLoading;
            _popupSceneLoading = Managers.ResourceManager.InstantiatePrefab(path, transform);
        }

        #endregion

        #region Functions

        public void EnablePop(GameObject popup)
        {
            _popupStack.Push(popup);
            AD.DebugLogger.Log($"_popupStack.Count: {_popupStack.Count}, 팝업 스택에 푸시됨");
        }
        
        public void DisablePop()
        {
            AD.Managers.SoundManager.UI_Click();

            if (_isException)
            {
                AD.DebugLogger.Log($"{_isException} - 예외 처리 활성");
                return;
            }

            if (_popupStack.Count > 0)
            {
                GameObject popup = _popupStack.Pop();
                AD.DebugLogger.Log($"_popupStack.Count: {_popupStack.Count} 팝업 스택에서 팝업 제거됨");
                popup.SetActive(false);
            }
            else
            {
                //if (!AD.Managers.GameM.IsGame)
                //{
                //    AD.DebugLogger.Log("PopupManager", "lobby scene -> quit popup");

                //    if (!_popupExit.activeSelf)
                //        PopupExit();
                //}
                //else
                //{
                //    AD.DebugLogger.Log("PopupManager", "game scene-> go lobby popup");

                //    if (!_popupLobby.activeSelf)
                //        PopupGoLobby();
                //}
            }
        }

        public void SetException() => _isException = true;

        public void ReleaseException() => _isException = false;

        public void SetFlow() => _isFlow = true;

        public void ReleaseFlow() => _isFlow = false;

        #endregion

        #region Common Popup

        public void PopupMessage(string message)
        {
            string path = GameConstants.GetPath(GameConstants.ResourceCategory.Popup) + CommonPopupType.PopupMessage;
            Managers.ResourceManager.InstantiatePrefab(path, transform, true).GetComponent<PopupMessage>()
                .SetMessage(message);
        }

        public void PopupLoading() => _popupLoading.SetActive(true);
        public void ClosePopupLoading() => _popupLoading.SetActive(false);

        public void PopupSceneLoading() => _popupSceneLoading.SetActive(true);
        public void ClosePopupSceneLoading() => _popupSceneLoading.SetActive(false);

        #endregion
    }
}