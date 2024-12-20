#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Fusion;

public class ContentManage : MonoBehaviour
{
    static ContentManage instance;
    public static ContentManage Instance { get { return instance; } }

    [Header("--- 세팅 ---")]
    [SerializeField] GameObject _go_room = null;
    [SerializeField] float _roomAmount = 0;
    [SerializeField, Tooltip("사용 될 ScrollView의 부모 패널 - [ 최대 사이즈(sizeDelta.y)를 알기 위함 ]")]
    RectTransform _RTR_parentView = null;
    [SerializeField, Tooltip("RectTransform - content")]
    RectTransform _RTR_content = null;
    [SerializeField, Tooltip("GridLayoutGroup - Init시 여러 계산에 필요")]
    GridLayoutGroup _GLG_content = null;
    [SerializeField, Tooltip("ContentSizeFitter - 아이템 생성 후 enabled false")]
    ContentSizeFitter _CSF_content = null;
    [SerializeField, Tooltip("더해줄 최소 생성 라인 수 [고정]")]
    int _minPlusLine = 4;

    [Header("--- 참고용 [ Content ] ---")]
    [SerializeField] List<GameObject> _list_room = new List<GameObject>();
    [SerializeField, Tooltip("최소 생성 RoomList")]
    LinkedList<AD.RoomObject> _LL_rooms = new LinkedList<AD.RoomObject>();
    [SerializeField, Tooltip("비활성 RoomList")]
    LinkedList<AD.RoomObject> _LL_enabledRooms = new LinkedList<AD.RoomObject>();

    [Header("--- 참고용 [ Content ] ---")]
    [SerializeField, Tooltip("Content의 PosY 변화 계산 위함")]
    float _curPosY = 0;
    [SerializeField, Tooltip("계산된 View의 총 Height")]
    float _contentHeight = 0;
    [SerializeField, Tooltip("첫 라인 Height")]
    float _startAnchorY = 0;
    [SerializeField, Tooltip("마지막 라인 Height")]
    float _endAnchorY = 0;
    [SerializeField, Tooltip("라인간 Height 간격")]
    float _intervalHeight = 0;
    [SerializeField, Tooltip("constraintCount의 따른 anchoredPositionX 배치")]
    List<float> _list_anchorX = new List<float>();
    [SerializeField, Tooltip("마지막 room Width_Index")]
    float _endAnchorX_index = 0;
    [SerializeField, Tooltip("마지막 room index")]
    int _endIndex = 0;
    [SerializeField, Tooltip("마지막 room index 참고만 하는 값")]
    float _org_endIndex = 0;

    [Header("--- 참고용 [ Level ] ---")]
    [SerializeField, Tooltip("현재 Content에 있는 room의 첫 index")]
    int _curContentstartIndex = 0;
    [SerializeField, Tooltip("현재 Content에 있는 room의 마지막 index")]
    int _curContentendIndex = 0;

    [Header("--- 세팅 [ Scroll Effect ] ---")]
    [SerializeField, Tooltip("GO - 가장 위로 이동")]
    GameObject _go_moveFirstRoomEffect = null;
    [SerializeField, Tooltip("GO - scroll시 활성화 -> block")]
    GameObject _go_blockScroll = null;
    [SerializeField, Range(0f, 1f), Tooltip("co_MoveFirstRoomEffect실행 시 lerp에 사용 될 값")]
    float _lerp = 0f;

    [Header("--- 참고용 [ Effect ] ---")]
    [Tooltip("MoveFirstRoomEffect()가 아직 진행중인지를 판단하기 위함")]
    bool _isMoveFirstRoomEffect = false;
    [Tooltip("Coroutine - co_MoveFirstRoom")]
    Coroutine _co_moveFirstRoom = null;

    [Header("--- 참고용 [ Photon Fusion ] ---")]
    List<SessionInfo> _list_sessionInfo = null;

    // ETC
    [Tooltip("스크롤 중인지 확인 -> 스크롤 끝난 뒤 체크하기 위함")]
    bool _isScroll = false;

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    public void Init(List<SessionInfo> sessionList)
    {
        _list_sessionInfo?.Clear();

        _list_sessionInfo = sessionList;
        _roomAmount = _list_sessionInfo.Count;

        if (_roomAmount > 0)
        {
            SetContentHeight();
            CreateTarget();
        }
    }

    private void Update()
    {
        if (CheckCurLevelInContent())
            _go_moveFirstRoomEffect.SetActive(false);
        else
            _go_moveFirstRoomEffect.SetActive(true);

        if (!_isScroll)
        {
            ContentManageUpLine();
            ContentManageDownLine();
        }
    }

    #region Functions

    #region Init

    /// <summary>
    /// Item, spacing, padding을 고려한 Content의 총 Height 계산
    /// 그외 필요한 부분 계산
    /// </summary>
    void SetContentHeight()
    {
        var lineCount = Math.Ceiling(_roomAmount / _GLG_content.constraintCount);

        float contentSize = (float)lineCount * _GLG_content.cellSize.y;
        float contentSpacingSize = (float)(lineCount - 1) * _GLG_content.spacing.y;
        float GLGTopBotPadding = _GLG_content.padding.top + _GLG_content.padding.bottom;

        _contentHeight = contentSize + contentSpacingSize + GLGTopBotPadding;

        _RTR_content.sizeDelta = new Vector2(_RTR_content.sizeDelta.x, _contentHeight);

        // cellSizeY + spacingY => 각 item의 간격
        _intervalHeight = _GLG_content.cellSize.y + _GLG_content.spacing.y;
    }

    /// <summary>
    /// room을 최소 라인수를 맞춰 생성
    /// 생성 후 필요한 값들 계산
    /// GLG, CSF를 비활성화
    /// </summary>
    void CreateTarget()
    {
        float height = _RTR_parentView.sizeDelta.y - _GLG_content.padding.top;

        int minLine = (int)Math.Truncate(height / (_GLG_content.cellSize.y + _GLG_content.spacing.y)) + _minPlusLine;

        int settingAmount = minLine * _GLG_content.constraintCount;
        int x = -1;

        if (_list_room.Count == 0)
        {
            for (int i = -1; ++i < settingAmount;)
            {
                // 최소 생성해야하는 양에 맞춰 생성 후 list에 보관
                GameObject room = Instantiate(_go_room, _RTR_content);
                _list_room.Add(room);
                room.SetActive(true);
            }
        }
        else
        {
            _LL_enabledRooms.Clear();
            _LL_rooms.Clear();
            _list_anchorX.Clear();

            foreach (GameObject room in _list_room)
                room.SetActive(false);

            for (int i = -1; ++i < settingAmount;)
                _list_room[i].SetActive(true);
        }

        for (int i = -1; ++i < settingAmount;)
        {
            AD.RoomObject roomObject = _list_room[i].GetComponent<AD.RoomObject>();
            roomObject.SetRoom(_list_sessionInfo[i], i);
            _LL_rooms.AddLast(roomObject);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_RTR_content);

            // 첫 줄의 room의 y 값 받음
            if (i == 0)
                _startAnchorY = roomObject._RTR_this.anchoredPosition.y;

            // 한 줄에서 나올 수 있는 x 값 받음
            if (++x < _GLG_content.constraintCount)
                _list_anchorX.Add(roomObject._RTR_this.anchoredPosition.x);

            // 최소로 생성하는 아이템의 양이 총 생성해야하는 양 보다 클 수 있음을 대비
            // 최종목적은 마지막 item의 anchoredPos + index
            if (i + 1 >= _roomAmount || i + 1 >= minLine * _GLG_content.constraintCount)
            {
                _endAnchorX_index = roomObject._RTR_this.anchoredPosition.x;
                _endAnchorY = roomObject._RTR_this.anchoredPosition.y;
                _org_endIndex = _endIndex = i;
                break;
            }
        }

        _curContentstartIndex = 0;
        _curContentendIndex = settingAmount - 1;

        // 받은 마지막 anchoredPos를 이용해 마지막 x의 index 구함
        for (int i = -1; ++i < _list_anchorX.Count;)
            if (_list_anchorX[i] == _endAnchorX_index)
                _endAnchorX_index = i;

        // 첫 세팅을 위해 사용한 GLG, CSF 비활성화
        _GLG_content.enabled = false;
        _CSF_content.enabled = false;
    }
    #endregion

    #region Scroll
    /// <summary>
    /// ScrollRect -> On Value Changed에서 호출
    /// * 생성된 아이템 관리
    /// </summary>
    public void SetRooms()
    {
        _isScroll = true;

        // 가장 위, 아래 부분 스크롤 시 제어 X
        // 스크롤 내리는 중
        if (_RTR_content.anchoredPosition.y > _curPosY && _RTR_content.anchoredPosition.y > 0)
        {
            _curPosY = _RTR_content.anchoredPosition.y;
            ContentManageUpLine();
        }

        // 스크롤 올리는 중
        if (_RTR_content.anchoredPosition.y < _curPosY && _RTR_content.anchoredPosition.y < _RTR_content.sizeDelta.y - _RTR_parentView.sizeDelta.y)
        {
            _curPosY = _RTR_content.anchoredPosition.y;
            ContentManageDownLine();
        }

        _startAnchorY = _LL_rooms.First.Value._RTR_this.anchoredPosition.y;
        _endAnchorY = _LL_rooms.Last.Value._RTR_this.anchoredPosition.y;

        _isScroll = false;
    }

    void ContentManageUpLine()
    {
        if (_endIndex + 1 >= _roomAmount)
            return;

        foreach (AD.RoomObject room in _LL_rooms)
        {
            if (room._RTR_this.anchoredPosition.y - _intervalHeight >= -_RTR_content.anchoredPosition.y)
            {
                _LL_enabledRooms.AddLast(room);
                room.gameObject.SetActive(false);
            }
            else
                break;
        }

        // 비활성화 한 item들을 _LL_items에서 지운 뒤 위치 조절 후 활성화
        if (_LL_enabledRooms != null && _LL_enabledRooms.Count > 0)
        {
            foreach (AD.RoomObject room in _LL_enabledRooms)
                _LL_rooms.Remove(room);

            foreach (AD.RoomObject room in _LL_enabledRooms)
            {
                if (_endIndex + 1 < _roomAmount)
                {
                    ++_endIndex;
                    ++_curContentendIndex;

                    room.SetRoom(_list_sessionInfo[_curContentendIndex], _curContentendIndex);
                    _LL_rooms.AddLast(room);

                    if (_endAnchorX_index + 1 >= _list_anchorX.Count)
                    {
                        _endAnchorX_index = 0;
                        _endAnchorY -= _intervalHeight;
                    }
                    else
                        ++_endAnchorX_index;

                    room._RTR_this.anchoredPosition = new Vector2(_list_anchorX[(int)_endAnchorX_index], _endAnchorY);

                    room.gameObject.SetActive(true);
                }
                else
                    break;
            }

            foreach (AD.RoomObject room in _LL_rooms)
                _LL_enabledRooms.Remove(room);
        }

        _curContentstartIndex = _LL_rooms.First.Value._sessionIndex;
    }

    void ContentManageDownLine()
    {
        // 가장 최상단 일 경우 return 
        if (_endIndex <= _org_endIndex)
            return;

        // Content의 윗 부분에 item이 있을 경우 return
        if (_LL_rooms.First.Value._RTR_this.anchoredPosition.x == _list_anchorX[0]
            && _LL_rooms.First.Value._RTR_this.anchoredPosition.y >= -_RTR_content.anchoredPosition.y)
            return;

        // _LL_enabledItems.Count를 _GLG_content.constraintCount와 맞춰주고
        while (true)
        {
            if (_LL_enabledRooms != null && _LL_enabledRooms.Count >= _GLG_content.constraintCount)
                break;

            _LL_rooms.Last.Value.gameObject.SetActive(false);
            _LL_enabledRooms.AddLast(_LL_rooms.Last.Value);
            _LL_rooms.RemoveLast();
            --_endIndex;
            --_curContentendIndex;
        }

        // 윗 라인을 채움
        if (_LL_enabledRooms != null && _LL_enabledRooms.Count >= _GLG_content.constraintCount)
        {
            _startAnchorY += _intervalHeight;

            for (int i = -1; ++i < _list_anchorX.Count;)
                if (_list_anchorX[i] == _LL_rooms.Last.Value._RTR_this.anchoredPosition.x)
                    _endAnchorX_index = i;

            int x = _GLG_content.constraintCount;
            foreach (AD.RoomObject room in _LL_enabledRooms)
            {
                if (--x >= 0)
                {
                    --_curContentstartIndex;
                    room.SetRoom(_list_sessionInfo[_curContentstartIndex], _curContentstartIndex);
                    _LL_rooms.AddFirst(room);

                    room._RTR_this.anchoredPosition = new Vector2(_list_anchorX[x], _startAnchorY);
                    room.gameObject.SetActive(true);
                }
            }

            foreach (AD.RoomObject room in _LL_rooms)
                _LL_enabledRooms.Remove(room);
        }
    }
    #endregion

    #region Scroll Effect
    /// <summary>
    /// 현재 레벨의 object가 Viewport상에 존재하는지 확인
    /// </summary>
    /// <returns></returns>
    bool CheckCurLevelInContent()
    {
        var curLine = Math.Ceiling(1f / _GLG_content.constraintCount);

        float curY = (float)curLine * _intervalHeight + _GLG_content.padding.top;

        if (_RTR_content.anchoredPosition.y >= curY
            || _RTR_content.anchoredPosition.y <= curY - _RTR_parentView.sizeDelta.y - _intervalHeight)
            return false;
        else
            return true;
    }

    /// <summary>
    /// ScrollView -> Viewport -> MoveFirstRoomEffect 클릭 시 Event Trigger로 호출
    /// </summary>
    public void MoveFirstRoomEffect()
    {
        if (!_isMoveFirstRoomEffect)
        {
            StopMoveCurLevelCoroutine();

            _co_moveFirstRoom = StartCoroutine(co_MoveFirstRoomEffect());
        }
    }

    /// <summary>
    /// ScrollView -> BlockScroll 클릭 시 Event Trigger로 호출
    /// </summary>
    public void BlockScroll()
    {
        StopMoveCurLevelCoroutine();
    }

    IEnumerator co_MoveFirstRoomEffect()
    {
        _isMoveFirstRoomEffect = true;
        _go_blockScroll.SetActive(true);

        var curLine = Math.Ceiling(1f / _GLG_content.constraintCount);
        float curY = ((float)curLine - 2) * _intervalHeight + _GLG_content.padding.top;
        if (curY < 0)
            curY = _GLG_content.padding.top / 2;

        Vector2 targetPosition = new Vector2(_RTR_content.anchoredPosition.x, curY);

        while (Math.Abs(curY - _RTR_content.anchoredPosition.y) >= 1f)
        {
            _RTR_content.anchoredPosition = Vector2.Lerp(_RTR_content.anchoredPosition, targetPosition, _lerp);
            yield return null;
        }

        _go_blockScroll.SetActive(false);
        _isMoveFirstRoomEffect = false;
    }

    void StopMoveCurLevelCoroutine()
    {
        if (_co_moveFirstRoom != null)
        {
            StopCoroutine(_co_moveFirstRoom);

            _go_blockScroll.SetActive(false);
            _isMoveFirstRoomEffect = false;

            _co_moveFirstRoom = null;
        }
    }
    #endregion

    #endregion

#if UNITY_EDITOR
    [CustomEditor(typeof(ContentManage))]
    public class customEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("dynamic scrollview\n" +
                "사용 할 content item과 Content의 Grid Layout Group을 세팅 뒤 사용", MessageType.Info);

            base.OnInspectorGUI();
        }
    }
#endif
}