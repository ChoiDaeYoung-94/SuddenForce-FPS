using UnityEngine;

namespace AD
{
    /// <summary>
    /// PopupObject는 팝업 관리와 관련된 컴포넌트로, 
    /// 활성화 시 PopupManager와 상호작용하여 flow 담당
    /// </summary>
    public class PopupObject : MonoBehaviour
    {
        private enum CheckType
        {
            Normal,
            Exception, // Exception인 경우, 팝업이 사라질 때 Release 처리
            OneShot // 닫힐 때 바로 Destroy 되는 일회성 팝업
        }

        [SerializeField] private CheckType _checkType = CheckType.Normal;

        private void OnEnable()
        {
            switch (_checkType)
            {
                case CheckType.Normal:
                    Managers.PopupManager.EnablePop(gameObject);
                    break;
                case CheckType.Exception:
                    Managers.PopupManager.SetException();
                    break;
            }
        }

        /// <summary>
        /// Normal 타입일 경우 팝업을 비활성화 처리합니다.
        /// </summary>
        public void DisablePop()
        {
            if (_checkType == CheckType.Normal)
            {
                Managers.PopupManager.DisablePop();
            }
            else if (_checkType == CheckType.OneShot)
            {
                Destroy(gameObject);
            }
        }

        private void OnDisable()
        {
            if (_checkType == CheckType.Exception)
            {
                Managers.PopupManager.ReleaseException();
            }
        }
    }
}