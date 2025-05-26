using Fusion;
using TMPro;
using UnityEngine;

namespace AD
{
    public class Room : MonoBehaviour
    {
        public RectTransform _thisRect;
        public int _sessionIndex = 0;

        // 그 외 접근 불가 데이터
        private SessionInfo _sessionInfo;
        [SerializeField] private TMP_Text _roomNameText;
        [SerializeField] private TMP_Text _gameSceneNameText;
        [SerializeField] private TMP_Text _roomPlayerCountText;
        
        public void SetRoom(SessionInfo sessionInfo, int sessionIndex)
        {
            _sessionInfo = sessionInfo;
            _sessionIndex = sessionIndex;

            _roomNameText.text = _sessionIndex.ToString();

            _roomNameText.text = _sessionInfo.Name;
            _gameSceneNameText.text = _sessionInfo.Properties["MapName"];
            _roomPlayerCountText.text = $"{_sessionInfo.PlayerCount} / {_sessionInfo.MaxPlayers}";
        }

        public void JoinRoom()
        {
            NetworkRunnerManager.Instance.JoinRoom(_sessionInfo.Name);
        }
    }
}
