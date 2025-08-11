using System;
using UnityEngine;

namespace AD
{
    /// <summary>
    /// GameObject 확장 메서드
    /// </summary>
    public static class Extension
    {
        /// <summary>
        /// GameObject에서 특정 컴포넌트를 가져오거나, 없으면 추가 후 반환
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            return Utility.GetOrAddComponent<T>(gameObject);
        }

        public static Component GetOrAddComponent(this GameObject gameObject, Type type)
        {
            var comp = gameObject.GetComponent(type);
            return comp != null ? comp : gameObject.AddComponent(type);
        }
    }
}