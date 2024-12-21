using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Fusion;

using TMPro;

namespace AD
{
    public class Room : MonoBehaviour
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

        internal void SetRoom(SessionInfo sessionInfo, int sessionIndex)
        {
            _sessionInfo = sessionInfo;
            _sessionIndex = sessionIndex;

            _TMP_roomName.text = _sessionIndex.ToString();

            _TMP_roomName.text = _sessionInfo.Name;
            _TMP_gameSceneName.text = _sessionInfo.Properties["Map"];
            _TMP_roomPlayerCount.text = $"{_sessionInfo.PlayerCount} / {_sessionInfo.MaxPlayers}";
        }

        internal int GetIndex()
        {
            return _sessionIndex;
        }
    }
}
