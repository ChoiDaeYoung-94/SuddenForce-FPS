#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AD
{
    public class Popup_Roulette : MonoBehaviour
    {
        [Header("--- 세팅 ---")]
        [SerializeField, Tooltip("RectTransform - Panel_Roulette - 룰렛의 z값 이용하여 스핀")]
        RectTransform _RTR_roulette = null;
        [SerializeField, Tooltip("GameObject - Spin")]
        GameObject _go_spin = null;
        [SerializeField, Tooltip("GameObject - Stop")]
        GameObject _go_stop = null;

        [Header("--- 값 세팅(Roulette 관련) ---")]
        [SerializeField, Tooltip("룰렛 회전 속도")]
        float _spinSpeed = 0;
        float _orgSpinSpeed = 0;
        [SerializeField, Tooltip("Spin 버튼 클릭 후 몇 바퀴 돌릴 것 인지")]
        int _numberOfTurn = 0;
        [SerializeField, Tooltip("목표 Item의 index - 시계 방향으로 첫 아이템이 index == 1이 됨")]
        int _targetIndex = 0;

        [Header("--- 참고용(아이템 개수에 따른 계산된 값) ---")]
        [SerializeField, Tooltip("아이템 개수")]
        int _itemAmount = 0;
        [SerializeField, Tooltip("아이템 개수에 따른 index rotation z 값")]
        float[] _indexRotationZ = null;
        [SerializeField, Tooltip("아이템 개수에 따른 index range")]
        float _indexRange = 0;
        [SerializeField, Tooltip("아이템 개수에 따른 index 범위 최대 random 값")]
        float _indexmaxRandom = 0;
        [SerializeField, Tooltip("계산된 목표 rotationZ 값")]
        float _targetRotationZ = 0;

        [Header("--- 참고용(Spin 시 계산에 필요) ---")]
        [SerializeField, Tooltip("몇 바퀴 도는 지 확인")]
        float _stack = 0;
        [SerializeField, Tooltip("_numberOfTurn 돈 뒤 가야할 RotationZ")]
        float _leftValue = 0;
        float _orgLeftValue = 0;

        /// <summary>
        /// 초기화 호출 위치 기입
        /// </summary>
        internal void Init()
        {
            _orgSpinSpeed = _spinSpeed;

            _itemAmount = _RTR_roulette.childCount;
            _indexRotationZ = new float[_itemAmount];
            _indexRange = 360f / _itemAmount;
            _indexmaxRandom = (_indexRange - ((_indexRange / 10f) * 2f)) / 2f;

            float temp = _indexRange;
            _indexRotationZ[0] = (temp + temp - _indexRange) / 2f;
            for (int i = 0; ++i < _itemAmount;)
                _indexRotationZ[i] = _indexRotationZ[i - 1] + _indexRange;

            _targetRotationZ = _indexRotationZ[_targetIndex - 1];
            float plusValue = Random.Range(0f, _indexmaxRandom);
            int random = Random.Range(1, 3);
            _targetRotationZ = random == 1 ? _targetRotationZ + plusValue : _targetRotationZ - plusValue;
        }

        /// <summary>
        /// 비활성화의 경우 PopM에서 진행
        /// </summary>
        private void OnDisable()
        {
            Reset();
        }

        #region Functions

        #region OnDisable
        /// <summary>
        /// OnDisable시 리셋
        /// </summary>
        private void Reset()
        {
            Managers.UpdateM._update -= RouletteSpinning;
            Managers.UpdateM._update -= StopSpinning;
            _stack = 0;
            _leftValue = 0;
            _spinSpeed = _orgSpinSpeed;

            _RTR_roulette.rotation = Quaternion.identity;
            _go_spin.SetActive(true);
            _go_stop.SetActive(false);
        }
        #endregion

        #region ETC
        /// <summary>
        /// Btn_Spin - 버튼 클릭 시
        /// </summary>
        public void Btn_Spin()
        {
            Managers.PopupM.SetException();

            Managers.UpdateM._update -= RouletteSpinning;
            Managers.UpdateM._update += RouletteSpinning;
        }

        /// <summary>
        /// 룰렛 회전
        /// </summary>
        void RouletteSpinning()
        {
            _RTR_roulette.eulerAngles = new Vector3(0, 0, _RTR_roulette.eulerAngles.z + _spinSpeed * UnityEngine.Time.deltaTime);
        }

        /// <summary>
        /// Btn_Stop - 버튼 클릭 시
        /// </summary>
        public void Btn_Stop()
        {
            _spinSpeed *= 1.5f;
            Managers.UpdateM._update -= RouletteSpinning;
            Managers.UpdateM._update -= StopSpinning;
            Managers.UpdateM._update += StopSpinning;
        }

        /// <summary>
        /// 서서히 원하는 지점으로 룰렛 멈춤
        /// </summary>
        void StopSpinning()
        {
            if (_stack <= 360f * _numberOfTurn)
            {
                float stack = _spinSpeed * UnityEngine.Time.deltaTime;
                _stack += stack;
                _RTR_roulette.eulerAngles = new Vector3(0, 0, _RTR_roulette.eulerAngles.z + stack);

                _orgLeftValue = _leftValue = _targetRotationZ - _RTR_roulette.eulerAngles.z + 360f;
            }
            else
            {
                float deceleration = _leftValue / _orgLeftValue;
                if (deceleration <= 0.1f)
                    deceleration = 0.1f;

                float stack = _spinSpeed * deceleration * UnityEngine.Time.deltaTime;
                if (_leftValue > 0)
                {
                    _leftValue -= stack;
                    if (_leftValue < 0)
                    {
                        stack -= Mathf.Abs(_leftValue);
                        Managers.UpdateM._update -= StopSpinning;
                        Managers.PopupM.ReleaseException();
                    }

                    _RTR_roulette.eulerAngles = new Vector3(0, 0, _RTR_roulette.eulerAngles.z + stack);
                }
            }
        }
        #endregion

        #endregion

#if UNITY_EDITOR
        [CustomEditor(typeof(Popup_Roulette))]
        public class customEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                EditorGUILayout.HelpBox("룰렛 관련 로직\n" +
                    "기존 Manager와 같이 사용 (Popup, Update관련)", MessageType.Info);

                base.OnInspectorGUI();
            }
        }
#endif
    }
}