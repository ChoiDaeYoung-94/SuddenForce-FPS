using UnityEngine;

namespace AD
{
    /// <summary>
    /// 타이틀 사이즈 기기대응
    /// </summary>
    [ExecuteInEditMode]
    public class ScreenSize : MonoBehaviour
    {
        [SerializeField] private RectTransform _titleObjectTransform;
        [SerializeField] private float _width = 1080f;
        [SerializeField] private float _height = 1920f;

        private void Update()
        {
            SetSize();
        }

        private void SetSize()
        {
            float ratio = _width / _height;
            float deviceRatio = _width / Screen.width;

            float x = 0f, y = 0f;

            // 디바이스의 가로 비율이 더 높을 경우
            if ((float)Screen.width / Screen.height >= ratio)
            {
                x = Screen.width * deviceRatio;
                y = _height * (x / _width);
            }
            // 디바이스의 세로 비율이 더 높을 경우
            else
            {
                y = Screen.height * deviceRatio;
                x = _width * (y / _height);
            }

            _titleObjectTransform.sizeDelta = new Vector2(x, y);
        }
    }
}
