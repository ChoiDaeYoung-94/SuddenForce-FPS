#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Linq;
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
    [SerializeField, Tooltip("content item")] GameObject _go_item = null;
    [SerializeField, Tooltip("사용 될 ScrollView의 부모 패널 - [ 최대 사이즈(sizeDelta.y)를 알기 위함 ]")]
    RectTransform _RTR_parentView = null;
    [SerializeField] RectTransform _RTR_content = null;
    [SerializeField] GridLayoutGroup _GLG_content = null;
    [SerializeField] ContentSizeFitter _CSF_content = null;
    [SerializeField] int _minPlusLine = 4;

    [Header("--- 세팅 [ Scroll Effect ] ---")]
    [SerializeField] GameObject _go_moveToTop = null;
    [SerializeField] GameObject _go_blockScroll = null;
    [SerializeField, Range(0f, 1f), Tooltip("_co_moveToTop 실행 시 lerp에 사용 될 값")] float _lerp = 0f;
    Coroutine _co_moveToTop = null;

    [Header("--- 참고용 ---")]
    List<AD.Room> _list_leastItemObject = new List<AD.Room>();
    LinkedList<AD.Room> _LL_activeItems = new LinkedList<AD.Room>();
    LinkedList<AD.Room> _LL_inActiveItems = new LinkedList<AD.Room>();
    [SerializeField] float _contentHeight = 0;
    [SerializeField] float _intervalHeight = 0;
    [SerializeField] List<float> _list_anchorX = new List<float>();
    [SerializeField] float _activeItemFirstLineAnchorY = 0;
    [SerializeField] float _activeItemLastLineAnchorY = 0;
    [SerializeField] float _anchorXIndexOflastActiveItem = 0;
    [SerializeField] int _activeItemFirstIndex = 0;
    [SerializeField] int _activeItemLastIndex = 0;

    // Session
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
        if (sessionList.Count == 0 && _list_leastItemObject.Count == 0)
            return;

        if (_list_leastItemObject.Count == 0)
        {
            _list_sessionList = sessionList;

            Settings();
            SetAnchorX();
            CreateContentItems();
        }
        else
            RefreshSessionList(sessionList);
    }

    private void Update()
    {
        if (_RTR_content.anchoredPosition.y >= 1000f)
            _go_moveToTop.SetActive(true);
        else
            _go_moveToTop.SetActive(false);

        if (_LL_activeItems.Count > 0)
        {
            ScrollDown();
            ScrollUp();
        }
    }

    #region Functions

    #region Init
    /// <summary>
    /// Item, spacing, padding을 고려한 Content의 총 Height 계산
    /// 그외 필요한 부분 계산
    /// </summary>
    private void Settings()
    {
        var lineCount = Math.Ceiling((float)_list_sessionList.Count / _GLG_content.constraintCount);

        float contentSize = (float)lineCount * _GLG_content.cellSize.y;
        float contentSpacingSize = (float)(lineCount - 1) * _GLG_content.spacing.y;
        float GLGTopBotPadding = _GLG_content.padding.top + _GLG_content.padding.bottom;

        _contentHeight = contentSize + contentSpacingSize + GLGTopBotPadding;

        _RTR_content.sizeDelta = new Vector2(_RTR_content.sizeDelta.x, _contentHeight);

        // cellSizeY + spacingY => 각 item의 간격
        _intervalHeight = _GLG_content.cellSize.y + _GLG_content.spacing.y;
    }

    private void SetAnchorX()
    {
        for (int i = -1; ++i < _GLG_content.constraintCount;)
        {
            float anchor = _GLG_content.padding.left + (_GLG_content.cellSize.x * i) + (_GLG_content.spacing.x * i);
            _list_anchorX.Add(anchor);
        }
    }

    /// <summary>
    /// Item을 최소 라인수를 맞춰 생성
    /// 생성 후 필요한 값들 계산
    /// 세팅 후 GLG, CSF를 비활성화
    /// </summary>
    private void CreateContentItems()
    {
        float height = _RTR_parentView.sizeDelta.y - _GLG_content.padding.top;

        int minLine = (int)Math.Truncate(height / (_GLG_content.cellSize.y + _GLG_content.spacing.y)) + _minPlusLine;

        int settingAmount = minLine * _GLG_content.constraintCount;
        for (int i = -1; ++i < settingAmount;)
        {
            AD.Room temp_room = Instantiate(_go_item, _RTR_content).GetComponent<AD.Room>();
            _list_leastItemObject.Add(temp_room);

            _LL_inActiveItems.AddLast(temp_room);

            temp_room.gameObject.SetActive(false);
        }

        settingAmount = settingAmount > _list_sessionList.Count ? _list_sessionList.Count : settingAmount;
        for (int i = -1; ++i < settingAmount;)
        {
            _list_leastItemObject[i].gameObject.SetActive(true);

            AD.Room item_room = _list_leastItemObject[i];
            item_room.SetRoom(_list_sessionList[i], i);

            _LL_activeItems.AddLast(item_room);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_RTR_content);
        }

        foreach (AD.Room room in _LL_activeItems)
            _LL_inActiveItems.Remove(room);

        _activeItemFirstIndex = 0;
        _activeItemLastIndex = settingAmount - 1;

        for (int i = -1; ++i < _list_anchorX.Count;)
            if (_list_anchorX[i] == _LL_activeItems.Last.Value._RTR_this.anchoredPosition.x)
            {
                _anchorXIndexOflastActiveItem = i;
                break;
            }

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
        if (_LL_activeItems.Count > 0)
        {
            _activeItemFirstLineAnchorY = _LL_activeItems.First.Value._RTR_this.anchoredPosition.y;
            _activeItemLastLineAnchorY = _LL_activeItems.Last.Value._RTR_this.anchoredPosition.y;
        }
    }

    /// <summary>
    /// 윗 라인을 지우고 아래 라인을 채움
    /// </summary>
    void ScrollDown()
    {
        if (_activeItemLastIndex + 1 >= _list_sessionList.Count)
            return;

        foreach (AD.Room item in _LL_activeItems)
        {
            if (item._RTR_this.anchoredPosition.y - _intervalHeight >= -_RTR_content.anchoredPosition.y)
            {
                _LL_inActiveItems.AddLast(item);
                item.gameObject.SetActive(false);
            }
            else
                break;
        }

        foreach (AD.Room item in _LL_inActiveItems)
            _LL_activeItems.Remove(item);

        if (_LL_inActiveItems.Count > 0)
        {
            foreach (AD.Room item in _LL_inActiveItems)
            {
                if (_activeItemLastIndex + 1 < _list_sessionList.Count)
                {
                    ++_activeItemLastIndex;

                    item.SetRoom(_list_sessionList[_activeItemLastIndex], _activeItemLastIndex);
                    _LL_activeItems.AddLast(item);

                    if (_anchorXIndexOflastActiveItem + 1 >= _list_anchorX.Count)
                    {
                        _anchorXIndexOflastActiveItem = 0;
                        _activeItemLastLineAnchorY -= _intervalHeight;
                    }
                    else
                        ++_anchorXIndexOflastActiveItem;

                    item._RTR_this.anchoredPosition = new Vector2(_list_anchorX[(int)_anchorXIndexOflastActiveItem], _activeItemLastLineAnchorY);

                    item.gameObject.SetActive(true);
                }
                else
                    break;
            }
        }

        foreach (AD.Room item in _LL_activeItems)
            _LL_inActiveItems.Remove(item);

        _activeItemFirstIndex = _LL_activeItems.First.Value._sessionIndex;
    }

    /// <summary>
    /// 아래 라인을 지우고 윗 라인을 채움
    /// </summary>
    void ScrollUp()
    {
        if (_activeItemFirstIndex == 0)
            return;

        // _LL_activeItems.First.Value가 화면의 아래로 내려가 위에 Item이 없을 경우에만 채우도록
        if (_LL_activeItems.First.Value._RTR_this.anchoredPosition.y >= -_RTR_content.anchoredPosition.y)
            return;

        // _LL_inActiveItems.Count를 _GLG_content.constraintCount와 맞춤 즉 아래 한 라인을 제거
        while (true)
        {
            if (_LL_inActiveItems.Count >= _GLG_content.constraintCount)
                break;

            _LL_activeItems.Last.Value.gameObject.SetActive(false);
            _LL_inActiveItems.AddLast(_LL_activeItems.Last.Value);
            _LL_activeItems.RemoveLast();
            --_activeItemLastIndex;
        }

        for (int i = -1; ++i < _list_anchorX.Count;)
            if (_list_anchorX[i] == _LL_activeItems.Last.Value._RTR_this.anchoredPosition.x)
            {
                _anchorXIndexOflastActiveItem = i;
                break;
            }

        if (_LL_inActiveItems.Count >= _GLG_content.constraintCount)
        {
            _activeItemFirstLineAnchorY += _intervalHeight;

            int x = _GLG_content.constraintCount;
            foreach (AD.Room item in _LL_inActiveItems)
            {
                if (--x >= 0)
                {
                    _LL_activeItems.AddFirst(item);

                    --_activeItemFirstIndex;
                    item.SetRoom(_list_sessionList[_activeItemFirstIndex], _activeItemFirstIndex);

                    item._RTR_this.anchoredPosition = new Vector2(_list_anchorX[x], _activeItemFirstLineAnchorY);
                    item.gameObject.SetActive(true);
                }
            }

            foreach (AD.Room item in _LL_activeItems)
                _LL_inActiveItems.Remove(item);
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

    #region Refresh Items
    private void RefreshSessionList(List<SessionInfo> newList)
    {
        for (int i = -1; ++i < _list_sessionList.Count;)
        {
            bool isRemoved = true;
            for (int j = -1; ++j < newList.Count;)
            {
                if (_list_sessionList[i].Name == newList[j].Name)
                {
                    isRemoved = false;
                    break;
                }
            }

            if (isRemoved)
            {
                RemoveSessionAt(i);
                _list_sessionList.RemoveAt(i);
                --i;
            }
        }

        for (int i = -1; ++i < newList.Count;)
        {
            bool isAdd = true;
            for (int j = -1; ++j < _list_sessionList.Count;)
            {
                if (newList[i].Name == _list_sessionList[j].Name)
                {
                    isAdd = false;
                    break;
                }
            }

            if (isAdd)
            {
                _list_sessionList.Insert(0, newList[i]);
                AddSessionAtTop();
            }
        }

        Settings();

        foreach (AD.Room room in _LL_activeItems)
            room.SetRoom(_list_sessionList[room._sessionIndex], room._sessionIndex);
    }

    /// <summary>
    /// 제거해야 할 List를 받고 현재 보이는 Item을 기준으로 제거
    /// 현재 보이는 Item의 뒷 부분이라면 무시 -> 마지막에 List 덮어 씌움
    /// </summary>
    public void RemoveSessionAt(int removeIndex)
    {
        if (removeIndex < _activeItemFirstIndex)
        {
            foreach (AD.Room item in _LL_activeItems)
                MoveActiveItemAfterRemoveItem(item);
        }
        else if (removeIndex <= _activeItemLastIndex)
        {
            AD.Room removedItem = FindItemByIndex(removeIndex);
            _LL_activeItems.Remove(removedItem);

            removedItem.gameObject.SetActive(false);
            _LL_inActiveItems.AddLast(removedItem);

            foreach (AD.Room item in _LL_activeItems)
            {
                int oldIndex = item._sessionIndex;

                if (oldIndex > removeIndex)
                    MoveActiveItemAfterRemoveItem(item);
            }
        }

        _activeItemFirstIndex = _LL_activeItems.First.Value._sessionIndex;
        _activeItemLastIndex = _LL_activeItems.Last.Value._sessionIndex;
    }

    /// <summary>
    /// 아이템이 추가될 경우 항상 가장 첫번째로(index => 0) 삽입 후 
    /// 기존 Item들의 Index를 ++
    /// </summary>
    public void AddSessionAtTop()
    {
        foreach (AD.Room room in _LL_activeItems)
            MoveActiveItemBeforeAddItem(room);

        if (_activeItemFirstIndex == 0)
        {
            if (_LL_inActiveItems.Count > 0)
            {
                AD.Room newItem = _LL_inActiveItems.First.Value;
                _LL_inActiveItems.RemoveFirst();

                newItem.SetRoom(_list_sessionList[0], 0);
                newItem.gameObject.SetActive(true);

                newItem._RTR_this.anchoredPosition =
                    new Vector2(_list_anchorX[0], -_GLG_content.padding.top);

                _LL_activeItems.AddFirst(newItem);
            }
            else
            {
                AD.Room newItem = _LL_activeItems.Last.Value;
                _LL_activeItems.RemoveLast();

                newItem.SetRoom(_list_sessionList[0], 0);

                newItem._RTR_this.anchoredPosition =
                    new Vector2(_list_anchorX[0], -_GLG_content.padding.top);

                _LL_activeItems.AddFirst(newItem);
            }
        }

        _activeItemFirstIndex = _LL_activeItems.First.Value._sessionIndex;
        _activeItemLastIndex = _LL_activeItems.Last.Value._sessionIndex;
    }

    private AD.Room FindItemByIndex(int index)
    {
        foreach (AD.Room item in _LL_activeItems)
        {
            if (item._sessionIndex == index)
                return item;
        }

        return null;
    }

    private void MoveActiveItemAfterRemoveItem(AD.Room item)
    {
        item._sessionIndex = item._sessionIndex - 1;

        for (int j = -1; ++j < _list_anchorX.Count;)
        {
            if (item._RTR_this.anchoredPosition.x == _list_anchorX[j])
            {
                if (j == 0)
                {
                    item._RTR_this.anchoredPosition =
                        new Vector2(_list_anchorX[_list_anchorX.Count - 1], item._RTR_this.anchoredPosition.y + _intervalHeight);

                    break;
                }
                else
                {
                    item._RTR_this.anchoredPosition =
                        new Vector2(_list_anchorX[j - 1], item._RTR_this.anchoredPosition.y);

                    break;
                }
            }
        }
    }

    private void MoveActiveItemBeforeAddItem(AD.Room item)
    {
        item._sessionIndex = item._sessionIndex + 1;

        for (int j = -1; ++j < _list_anchorX.Count;)
        {
            if (item._RTR_this.anchoredPosition.x == _list_anchorX[j])
            {
                if (j == _list_anchorX.Count - 1)
                {
                    item._RTR_this.anchoredPosition =
                        new Vector2(_list_anchorX[0], item._RTR_this.anchoredPosition.y - _intervalHeight);

                    break;
                }
                else
                {
                    item._RTR_this.anchoredPosition =
                        new Vector2(_list_anchorX[j + 1], item._RTR_this.anchoredPosition.y);

                    break;
                }
            }
        }
    }

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