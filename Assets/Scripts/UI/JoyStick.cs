using UnityEngine;
using UnityEngine.EventSystems;

public class JoyStick : MonoBehaviour
{
    private enum Mode
    {
        Null,
        FixedArea,
        FreeArea
    }

    [Header("JoyStick 관련 세팅")]
    [SerializeField] private Mode _mode = Mode.Null;
    [SerializeField] private RectTransform _handleTransform;
    [SerializeField] private RectTransform _handleAreaTransform;
    private Vector3 _joystickVector = Vector3.zero;
    private float _joystickDistance = 0;
    public Vector2 Direction => new Vector2(_joystickVector.x, _joystickVector.y);
    public float Magnitude => _joystickDistance;
    private float _handleAreaRadius = 0;
    private Vector3 _firstTouchPosition = Vector3.zero;
    private Vector3 _distanceVector = Vector3.zero;

    private void Awake()
    {
        _handleAreaRadius = _handleAreaTransform.sizeDelta.y * 1.3f;
        _firstTouchPosition = _handleTransform.position;
        _distanceVector = _handleTransform.position - _handleAreaTransform.position;

        if (_mode == Mode.FixedArea)
        {
            _handleAreaTransform.gameObject.SetActive(true);
        }
        else if (_mode == Mode.FreeArea)
        {
            _handleAreaTransform.gameObject.SetActive(false);
        }
    }

    #region EventTrigger
    public void PointDown(BaseEventData baseEventData)
    {
        PointerEventData pointerEventData = baseEventData as PointerEventData;
        Vector3 inputPos = pointerEventData.position;

        if (_mode == Mode.FreeArea)
        {
            _firstTouchPosition = inputPos;
            _handleAreaTransform.position = inputPos - _distanceVector;
            _handleAreaTransform.gameObject.SetActive(true);
        }

        _handleTransform.position = inputPos;
        _joystickVector = (inputPos - _firstTouchPosition).normalized;
        _joystickDistance = Vector3.Distance(inputPos, _firstTouchPosition);
    }

    public void Drag(BaseEventData baseEventData)
    {
        PointerEventData pointerEventData = baseEventData as PointerEventData;
        Vector3 dragPosition = pointerEventData.position;
        _joystickVector = (dragPosition - _firstTouchPosition).normalized;
        _joystickDistance = Vector3.Distance(dragPosition, _firstTouchPosition);

        if (_joystickDistance > _handleAreaRadius)
            _joystickDistance = _handleAreaRadius;

        _handleTransform.position = Vector3.Lerp(_handleTransform.position, _firstTouchPosition + _joystickVector * _joystickDistance, 0.7f);
    }

    public void PointUp(BaseEventData baseEventData)
    {
        _handleTransform.anchoredPosition = Vector2.zero;

        if (_mode == Mode.FreeArea)
            _handleAreaTransform.gameObject.SetActive(false);
    }
    #endregion
}