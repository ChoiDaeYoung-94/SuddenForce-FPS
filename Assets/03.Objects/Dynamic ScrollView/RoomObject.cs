#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Fusion;

using TMPro;

namespace AD
{
    public class RoomObject : MonoBehaviour
    {
        [Header("--- 세팅 ---")]
        [SerializeField, Tooltip("Content에 RectTransform을 넘겨주기 위함")]
        internal RectTransform _RTR_this = null;
        [SerializeField] TMP_Text _TMP_roomName = null;
        [SerializeField] TMP_Text _TMP_gameSceneName = null;
        [SerializeField] TMP_Text _TMP_roomPlayerCount = null;

        [Header("--- 참고용 ---")]
        public SessionInfo _sessionInfo = null;
        public int _sessionIndex = 0;

        internal void SetRoom(SessionInfo sessionInfo, int index)
        {
            _sessionInfo = sessionInfo;
            _sessionIndex = index;

            _TMP_roomName.text = _sessionInfo.Name;
            _TMP_roomPlayerCount.text = $"{_sessionInfo.PlayerCount} / {_sessionInfo.MaxPlayers}";
            _TMP_gameSceneName.text = $"{_sessionInfo.Properties["Map"]}";
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
