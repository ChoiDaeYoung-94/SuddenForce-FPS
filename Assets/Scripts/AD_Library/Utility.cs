using MiniJSON;
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
            if (string.IsNullOrEmpty(text.text) || text.text == "" || text.text.Length <= 1 ||
                text.text.Replace(" ", "").Length == 1)
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
}