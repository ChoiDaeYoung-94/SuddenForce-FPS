using System.Collections.Generic;
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
                if (Input.GetKeyDown(KeyCode.Escape))
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
            DebugLogger.Log($"_popupStack.Count: {_popupStack.Count}, 팝업 스택에 푸시됨");
        }
        
        public void DisablePop()
        {
            if (_isException)
            {
                DebugLogger.Log($"{_isException} - 예외 처리 활성");
                return;
            }

            if (_popupStack.Count > 0)
            {
                GameObject popup = _popupStack.Pop();
                DebugLogger.Log($"_popupStack.Count: {_popupStack.Count} 팝업 스택에서 팝업 제거됨");
                popup.SetActive(false);
            }
            else
            {
                //if (!Managers.GameM.IsGame)
                //{
                //    DebugLogger.Log("PopupManager", "lobby scene -> quit popup");

                //    if (!_popupExit.activeSelf)
                //        PopupExit();
                //}
                //else
                //{
                //    DebugLogger.Log("PopupManager", "game scene-> go lobby popup");

                //    if (!_popupLobby.activeSelf)
                //        PopupGoLobby();
                //}
            }
        }

        public void SetException() => _isException = true;

        public void ReleaseException() => _isException = false;

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