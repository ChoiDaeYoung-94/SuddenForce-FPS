#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

namespace AD
{
    public class RoomObject : MonoBehaviour
    {
        [Header("--- 세팅 ---")]
        [SerializeField, Tooltip("Content에 RectTransform을 넘겨주기 위함")]
        internal RectTransform _RTR_this = null;
        [SerializeField, Tooltip("TMP_Text - level표기 위함")]
        TMP_Text _TMP_level = null;
        [Header("--- 참고용 ---")]
        [SerializeField, Tooltip("현재 이 object의 level")]
        internal int _curLevel = 0;

        internal void SetLevel(int level)
        {
            _curLevel = level;
            _TMP_level.text = level.ToString();
        }

        internal int GetLevel()
        {
            return _curLevel;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(RoomObject))]
    public class customEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Room 관련", MessageType.Info);

            base.OnInspectorGUI();
        }
    }
#endif
}
