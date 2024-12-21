#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Fusion;

public class RoomManage : MonoBehaviour
{
    static RoomManage instance;
    public static RoomManage Instance { get { return instance; } }

    [Header("--- 세팅 [ Content ] ---")]
    [SerializeField, Tooltip("GO - 사용 될 content item")]
    GameObject _go_item = null;
    [SerializeField, Tooltip("필요한 item의 amount - 높이 계산에 필요")]
    float _amount = 0;
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
    [SerializeField] List<RectTransform> _list_room = new List<RectTransform>();
    [SerializeField, Tooltip("최소 생성 itemList")]
    LinkedList<AD.Room> _LL_items = new LinkedList<AD.Room>();
    [SerializeField, Tooltip("비활성 itemList")]
    LinkedList<AD.Room> _LL_enabledItems = new LinkedList<AD.Room>();

    [Header("--- 참고용 [ Content ] ---")]
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
    [SerializeField, Tooltip("마지막 item Width_Index")]
    float _endAnchorX_index = 0;
    [SerializeField, Tooltip("마지막 item index 참고만 하는 값")]
    float _org_endIndex = 0;

    [Header("--- 참고용 [ Room ] ---")]
    [SerializeField, Tooltip("현재 Content에 있는 room의 첫 index")]
    int _curContentRoomFirstIndex = 0;
    [SerializeField, Tooltip("현재 Content에 있는 Item_level의 마지막 index의 level")]
    int _curContentRoomEndIndex = 0;

    [Header("--- 세팅 [ Scroll Effect ] ---")]
    [SerializeField, Tooltip("GO - 현재 레벨이 있는 위치로 이동")]
    GameObject _go_moveToTop = null;
    [SerializeField, Tooltip("GO - scroll시 활성화 -> block")]
    GameObject _go_blockScroll = null;
    [SerializeField, Range(0f, 1f),
     Tooltip("_co_moveToTop 실행 시 lerp에 사용 될 값")]
    float _lerp = 0f;
    Coroutine _co_moveToTop = null;

    List<SessionInfo> _list_sessionList = null;

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
        _list_sessionList?.Clear();
        _list_sessionList = sessionList;

        _amount = _list_sessionList.Count;

        ResetData();
        SetContentHeight();
        CreateContentItems();
    }

    private void Update()
    {
        if (_RTR_content.anchoredPosition.y >= 1000f)
            _go_moveToTop.SetActive(true);
        else
            _go_moveToTop.SetActive(false);

        ContentManageUpLine();
        ContentManageDownLine();
    }

    #region Functions

    #region Init
    private void ResetData()
    {
        _GLG_content.enabled = true;
        _CSF_content.enabled = true;

        _RTR_content.anchoredPosition = new Vector2(_RTR_content.anchoredPosition.x, 0f);

        _list_anchorX?.Clear();
        _LL_items?.Clear();
        _LL_enabledItems?.Clear();

        foreach (RectTransform room in _list_room)
            room.gameObject.SetActive(false);

        foreach (RectTransform room in _list_room)
            room.SetParent(_RTR_parentView);

        foreach (RectTransform room in _list_room)
            room.SetParent(_RTR_content);
    }

    /// <summary>
    /// Item, spacing, padding을 고려한 Content의 총 Height 계산
    /// 그외 필요한 부분 계산
    /// </summary>
    private void SetContentHeight()
    {
        var lineCount = Math.Ceiling(_amount / _GLG_content.constraintCount);

        float contentSize = (float)lineCount * _GLG_content.cellSize.y;
        float contentSpacingSize = (float)(lineCount - 1) * _GLG_content.spacing.y;
        float GLGTopBotPadding = _GLG_content.padding.top + _GLG_content.padding.bottom;

        _contentHeight = contentSize + contentSpacingSize + GLGTopBotPadding;

        _RTR_content.sizeDelta = new Vector2(_RTR_content.sizeDelta.x, _contentHeight);

        // cellSizeY + spacingY => 각 item의 간격
        _intervalHeight = _GLG_content.cellSize.y + _GLG_content.spacing.y;
    }

    /// <summary>
    /// Item을 최소 라인수를 맞춰 생성
    /// 생성 후 필요한 값들 계산
    /// GLG, CSF를 비활성화
    /// </summary>
    private void CreateContentItems()
    {
        float height = _RTR_parentView.sizeDelta.y - _GLG_content.padding.top;

        int minLine = (int)Math.Truncate(height / (_GLG_content.cellSize.y + _GLG_content.spacing.y)) + _minPlusLine;

        int settingAmount = minLine * _GLG_content.constraintCount;
        if (_list_room.Count < settingAmount)
            while (_list_room.Count != settingAmount)
            {
                // 최소 생성해야하는 양에 맞춰 생성 후 list에도 보관
                GameObject item = Instantiate(_go_item, _RTR_content);
                item.SetActive(false);
                _list_room.Add(item.GetComponent<RectTransform>());
            }

        settingAmount = settingAmount > (int)_amount ? (int)_amount : settingAmount;

        int x = -1;
        for (int i = -1; ++i < settingAmount;)
        {
            _list_room[i].gameObject.SetActive(true);

            AD.Room item_room = _list_room[i].GetComponent<AD.Room>();
            item_room.SetRoom(_list_sessionList[i], i);

            _LL_items.AddLast(item_room);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_RTR_content);

            // 첫 줄의 item의 y 값 받음
            if (i == 0)
                _startAnchorY = item_room._RTR_this.anchoredPosition.y;

            // 한 줄에서 나올 수 있는 x 값 받음
            if (++x < _GLG_content.constraintCount)
                _list_anchorX.Add(item_room._RTR_this.anchoredPosition.x);

            // 최소로 생성하는 아이템의 양이 총 생성해야하는 양 보다 클 수 있음을 대비
            // 최종목적은 마지막 item의 anchoredPos + index
            if (i + 1 >= _amount || i + 1 >= minLine * _GLG_content.constraintCount)
            {
                _endAnchorX_index = item_room._RTR_this.anchoredPosition.x;
                _endAnchorY = item_room._RTR_this.anchoredPosition.y;
                _org_endIndex = _curContentRoomEndIndex = i;
                break;
            }
        }

        _curContentRoomFirstIndex = 0;
        _curContentRoomEndIndex = settingAmount - 1;

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
    /// </summary>
    public void SetContentItems()
    {
        if (_LL_items.Count > 0)
        {
            _startAnchorY = _LL_items.First.Value._RTR_this.anchoredPosition.y;
            _endAnchorY = _LL_items.Last.Value._RTR_this.anchoredPosition.y;
        }
    }

    void ContentManageUpLine()
    {
        if (_curContentRoomEndIndex + 1 >= _amount)
            return;

        foreach (AD.Room item in _LL_items)
        {
            if (item._RTR_this.anchoredPosition.y - _intervalHeight >= -_RTR_content.anchoredPosition.y)
            {
                _LL_enabledItems.AddLast(item);
                item.gameObject.SetActive(false);
            }
            else
                break;
        }

        // 비활성화 한 item들을 _LL_items에서 지운 뒤 위치 조절 후 활성화
        if (_LL_enabledItems != null && _LL_enabledItems.Count > 0)
        {
            foreach (AD.Room item in _LL_enabledItems)
                _LL_items.Remove(item);

            foreach (AD.Room item in _LL_enabledItems)
            {
                if (_curContentRoomEndIndex + 1 < _amount)
                {
                    ++_curContentRoomEndIndex;

                    item.SetRoom(_list_sessionList[_curContentRoomEndIndex], _curContentRoomEndIndex);
                    _LL_items.AddLast(item);

                    if (_endAnchorX_index + 1 >= _list_anchorX.Count)
                    {
                        _endAnchorX_index = 0;
                        _endAnchorY -= _intervalHeight;
                    }
                    else
                        ++_endAnchorX_index;

                    item._RTR_this.anchoredPosition = new Vector2(_list_anchorX[(int)_endAnchorX_index], _endAnchorY);

                    item.gameObject.SetActive(true);
                }
                else
                    break;
            }

            foreach (AD.Room item in _LL_items)
                _LL_enabledItems.Remove(item);
        }

        _curContentRoomFirstIndex = _LL_items.First.Value.GetIndex();
    }

    void ContentManageDownLine()
    {
        // 가장 최상단 일 경우 return 
        if (_curContentRoomEndIndex <= _org_endIndex)
            return;

        // Content의 윗 부분에 item이 있을 경우 return
        if (_LL_items.First.Value._RTR_this.anchoredPosition.x == _list_anchorX[0]
            && _LL_items.First.Value._RTR_this.anchoredPosition.y >= -_RTR_content.anchoredPosition.y)
            return;

        // _LL_enabledItems.Count를 _GLG_content.constraintCount와 맞춰주고
        while (true)
        {
            if (_LL_enabledItems != null && _LL_enabledItems.Count >= _GLG_content.constraintCount)
                break;

            _LL_items.Last.Value.gameObject.SetActive(false);
            _LL_enabledItems.AddLast(_LL_items.Last.Value);
            _LL_items.RemoveLast();
            --_curContentRoomEndIndex;
        }

        // 윗 라인을 채움
        if (_LL_enabledItems != null && _LL_enabledItems.Count >= _GLG_content.constraintCount)
        {
            _startAnchorY += _intervalHeight;

            for (int i = -1; ++i < _list_anchorX.Count;)
                if (_list_anchorX[i] == _LL_items.Last.Value._RTR_this.anchoredPosition.x)
                    _endAnchorX_index = i;

            int x = _GLG_content.constraintCount;
            foreach (AD.Room item in _LL_enabledItems)
            {
                if (--x >= 0)
                {
                    _LL_items.AddFirst(item);

                    --_curContentRoomFirstIndex;
                    item.SetRoom(_list_sessionList[_curContentRoomFirstIndex], _curContentRoomFirstIndex);

                    item._RTR_this.anchoredPosition = new Vector2(_list_anchorX[x], _startAnchorY);
                    item.gameObject.SetActive(true);
                }
            }

            foreach (AD.Room item in _LL_items)
                _LL_enabledItems.Remove(item);
        }
    }
    #endregion

    #region Scroll Effect
    /// <summary>
    /// ScrollView -> Viewport -> MoveCurLevelEffect 클릭 시 Event Trigger로 호출
    /// </summary>
    public void MoveToTop()
    {
        _co_moveToTop = StartCoroutine(co_MoveToTop());
    }

    IEnumerator co_MoveToTop()
    {
        _go_blockScroll.SetActive(true);

        while (_RTR_content.anchoredPosition.y >= 1f)
        {
            _RTR_content.anchoredPosition = Vector2.Lerp(_RTR_content.anchoredPosition, new Vector2(_RTR_content.anchoredPosition.x, 0f), _lerp);
            yield return null;
        }

        StopMoveToTopCoroutine();
    }

    void StopMoveToTopCoroutine()
    {
        if (_co_moveToTop != null)
        {
            StopCoroutine(_co_moveToTop);
            _co_moveToTop = null;
        }

        _go_blockScroll.SetActive(false);
    }

    /// <summary>
    /// ScrollView -> BlockScroll 클릭 시 Event Trigger로 호출
    /// </summary>
    public void BlockScroll() => StopMoveToTopCoroutine();
    #endregion

    #endregion

#if UNITY_EDITOR
    [CustomEditor(typeof(RoomManage))]
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