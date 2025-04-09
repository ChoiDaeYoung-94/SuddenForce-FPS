using System;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;
using UnityEngine.UI;

using Cysharp.Threading.Tasks;

using Fusion;

public class RoomManage : MonoBehaviour
{
    private static RoomManage _instance;
    public static RoomManage Instance { get { return _instance; } }

    [Header("--- RoomManage data ---")]
    [SerializeField] private GameObject _contentItem;
    [SerializeField] private RectTransform _scrollViewParentRect; // 최대 사이즈(sizeDelta.y)
    [SerializeField] private RectTransform _contentRect;
    [SerializeField] private GridLayoutGroup _contentGridLayoutGroup;
    [SerializeField] private ContentSizeFitter _contentContentSizeFitter;
    [SerializeField] private int _minimumPlusLine = 4;
    [SerializeField] private GameObject _moveToTop;
    [SerializeField] private GameObject _blockScroll;
    [SerializeField, Range(0f, 1f)] private float _lerp = 0f;   // MoveToTop lerp에 사용 될 값
    private CancellationTokenSource _ctsMoveToTop;

    // 그 외 접근 불가 데이터
    private List<AD.Room> _leastItemObjectList = new List<AD.Room>();
    private LinkedList<AD.Room> _activeItemsList = new LinkedList<AD.Room>();
    private LinkedList<AD.Room> _inActiveItemsList = new LinkedList<AD.Room>();
    private float _contentHeight = 0f;
    private float _intervalHeight = 0f;
    private List<float> _anchorXList = new List<float>();
    private float _activeItemFirstLineAnchorY = 0f;
    private float _activeItemLastLineAnchorY = 0f;
    private float _anchorXIndexOflastActiveItem = 0f;
    private int _activeItemFirstIndex = 0;
    private int _activeItemLastIndex = 0;

    // Session
    private List<SessionInfo> _sessionList = null;

    private void Awake()
    {
        _instance = this;

        _ctsMoveToTop = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        _ctsMoveToTop?.Cancel();
        _ctsMoveToTop?.Dispose();
        _ctsMoveToTop = null;

        _instance = null;
    }

    public void Init(List<SessionInfo> sessionList)
    {
        if (sessionList.Count == 0 && _leastItemObjectList.Count == 0)
            return;

        if (_leastItemObjectList.Count == 0)
        {
            _sessionList = sessionList;

            Settings();
            SetAnchorX();
            CreateContentItems();
        }
        else
            RefreshSessionList(sessionList);
    }

    private void Update()
    {
        if (_contentRect.anchoredPosition.y >= 1000f)
            _moveToTop.SetActive(true);
        else
            _moveToTop.SetActive(false);

        if (_activeItemsList.Count > 0)
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
        var lineCount = Math.Ceiling((float)_sessionList.Count / _contentGridLayoutGroup.constraintCount);

        float contentSize = (float)lineCount * _contentGridLayoutGroup.cellSize.y;
        float contentSpacingSize = (float)(lineCount - 1) * _contentGridLayoutGroup.spacing.y;
        float GLGTopBotPadding = _contentGridLayoutGroup.padding.top + _contentGridLayoutGroup.padding.bottom;

        _contentHeight = contentSize + contentSpacingSize + GLGTopBotPadding;

        _contentRect.sizeDelta = new Vector2(_contentRect.sizeDelta.x, _contentHeight);

        // cellSizeY + spacingY => 각 item의 간격
        _intervalHeight = _contentGridLayoutGroup.cellSize.y + _contentGridLayoutGroup.spacing.y;
    }

    private void SetAnchorX()
    {
        for (int i = 0; i < _contentGridLayoutGroup.constraintCount; i++)
        {
            float anchor = _contentGridLayoutGroup.padding.left + (_contentGridLayoutGroup.cellSize.x * i) + (_contentGridLayoutGroup.spacing.x * i);
            _anchorXList.Add(anchor);
        }
    }

    /// <summary>
    /// Item을 최소 라인수를 맞춰 생성
    /// 생성 후 필요한 값들 계산
    /// 세팅 후 GLG, CSF를 비활성화
    /// </summary>
    private void CreateContentItems()
    {
        float height = _scrollViewParentRect.sizeDelta.y - _contentGridLayoutGroup.padding.top;

        int minLine = (int)Math.Truncate(height / (_contentGridLayoutGroup.cellSize.y + _contentGridLayoutGroup.spacing.y)) + _minimumPlusLine;

        int settingAmount = minLine * _contentGridLayoutGroup.constraintCount;
        for (int i = 0; i < settingAmount; i++)
        {
            AD.Room temp_room = Instantiate(_contentItem, _contentRect).GetComponent<AD.Room>();
            _leastItemObjectList.Add(temp_room);

            _inActiveItemsList.AddLast(temp_room);

            temp_room.gameObject.SetActive(false);
        }

        settingAmount = settingAmount > _sessionList.Count ? _sessionList.Count : settingAmount;
        for (int i = 0; i < settingAmount; i++)
        {
            _leastItemObjectList[i].gameObject.SetActive(true);

            AD.Room item_room = _leastItemObjectList[i];
            item_room.SetRoom(_sessionList[i], i);

            _activeItemsList.AddLast(item_room);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_contentRect);
        }

        foreach (AD.Room room in _activeItemsList)
            _inActiveItemsList.Remove(room);

        _activeItemFirstIndex = 0;
        _activeItemLastIndex = settingAmount - 1;

        for (int i = 0; i < _anchorXList.Count; i++)
            if (_anchorXList[i] == _activeItemsList.Last.Value._thisRect.anchoredPosition.x)
            {
                _anchorXIndexOflastActiveItem = i;
                break;
            }

        _contentGridLayoutGroup.enabled = false;
        _contentContentSizeFitter.enabled = false;
    }

    #endregion

    #region Scroll

    /// <summary>
    /// ScrollRect -> On Value Changed에서 호출
    /// </summary>
    public void SetContentItems()
    {
        if (_activeItemsList.Count > 0)
        {
            _activeItemFirstLineAnchorY = _activeItemsList.First.Value._thisRect.anchoredPosition.y;
            _activeItemLastLineAnchorY = _activeItemsList.Last.Value._thisRect.anchoredPosition.y;
        }
    }

    /// <summary>
    /// 윗 라인을 지우고 아래 라인을 채움
    /// </summary>
    void ScrollDown()
    {
        if (_activeItemLastIndex + 1 >= _sessionList.Count)
            return;

        foreach (AD.Room item in _activeItemsList)
        {
            if (item._thisRect.anchoredPosition.y - _intervalHeight >= -_contentRect.anchoredPosition.y)
            {
                _inActiveItemsList.AddLast(item);
                item.gameObject.SetActive(false);
            }
            else
                break;
        }

        foreach (AD.Room item in _inActiveItemsList)
            _activeItemsList.Remove(item);

        if (_inActiveItemsList.Count > 0)
        {
            foreach (AD.Room item in _inActiveItemsList)
            {
                if (_activeItemLastIndex + 1 < _sessionList.Count)
                {
                    ++_activeItemLastIndex;

                    item.SetRoom(_sessionList[_activeItemLastIndex], _activeItemLastIndex);
                    _activeItemsList.AddLast(item);

                    if (_anchorXIndexOflastActiveItem + 1 >= _anchorXList.Count)
                    {
                        _anchorXIndexOflastActiveItem = 0;
                        _activeItemLastLineAnchorY -= _intervalHeight;
                    }
                    else
                        ++_anchorXIndexOflastActiveItem;

                    item._thisRect.anchoredPosition = new Vector2(_anchorXList[(int)_anchorXIndexOflastActiveItem], _activeItemLastLineAnchorY);

                    item.gameObject.SetActive(true);
                }
                else
                    break;
            }
        }

        foreach (AD.Room item in _activeItemsList)
            _inActiveItemsList.Remove(item);

        _activeItemFirstIndex = _activeItemsList.First.Value._sessionIndex;
    }

    /// <summary>
    /// 아래 라인을 지우고 윗 라인을 채움
    /// </summary>
    void ScrollUp()
    {
        if (_activeItemFirstIndex == 0)
            return;

        // _activeItemsList.First.Value가 화면의 아래로 내려가 위에 Item이 없을 경우에만 채우도록
        if (_activeItemsList.First.Value._thisRect.anchoredPosition.y >= -_contentRect.anchoredPosition.y)
            return;

        // _inActiveItemsList.Count를 _contentGridLayoutGroup.constraintCount와 맞춤 즉 아래 한 라인을 제거
        while (_inActiveItemsList.Count >= _contentGridLayoutGroup.constraintCount == false)
        {
            _activeItemsList.Last.Value.gameObject.SetActive(false);
            _inActiveItemsList.AddLast(_activeItemsList.Last.Value);
            _activeItemsList.RemoveLast();
            --_activeItemLastIndex;
        }

        for (int i = 0; i < _anchorXList.Count; i++)
        {
            if (_anchorXList[i] == _activeItemsList.Last.Value._thisRect.anchoredPosition.x)
            {
                _anchorXIndexOflastActiveItem = i;
                break;
            }
        }

        if (_inActiveItemsList.Count >= _contentGridLayoutGroup.constraintCount)
        {
            _activeItemFirstLineAnchorY += _intervalHeight;

            int x = _contentGridLayoutGroup.constraintCount;
            foreach (AD.Room item in _inActiveItemsList)
            {
                if (--x >= 0)
                {
                    _activeItemsList.AddFirst(item);

                    --_activeItemFirstIndex;
                    item.SetRoom(_sessionList[_activeItemFirstIndex], _activeItemFirstIndex);

                    item._thisRect.anchoredPosition = new Vector2(_anchorXList[x], _activeItemFirstLineAnchorY);
                    item.gameObject.SetActive(true);
                }
            }

            foreach (AD.Room item in _activeItemsList)
                _inActiveItemsList.Remove(item);
        }
    }

    #endregion

    #region Scroll Effect

    /// <summary>
    /// ScrollView -> Viewport -> MoveCurLevelEffect 클릭 시 Event Trigger로 호출
    /// </summary>
    public void MoveToTop() => MoveToTopAsync(_ctsMoveToTop.Token).Forget();

    private async UniTask MoveToTopAsync(CancellationToken token)
    {
        _blockScroll.SetActive(true);

        while (_contentRect.anchoredPosition.y >= 1f && !token.IsCancellationRequested)
        {
            _contentRect.anchoredPosition = Vector2.Lerp(_contentRect.anchoredPosition, new Vector2(_contentRect.anchoredPosition.x, 0f), _lerp);
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        _blockScroll.SetActive(false);
    }

    /// <summary>
    /// ScrollView -> BlockScroll 클릭 시 Event Trigger로 호출
    /// </summary>
    public void BlockScroll() => _ctsMoveToTop.Cancel();

    #endregion

    #region Refresh Items

    private void RefreshSessionList(List<SessionInfo> newList)
    {
        for (int i = 0; i < _sessionList.Count; i++)
        {
            bool isRemoved = true;
            for (int j = 0; j < newList.Count; j++)
            {
                if (_sessionList[i].Name == newList[j].Name)
                {
                    isRemoved = false;
                    break;
                }
            }

            if (isRemoved)
            {
                RemoveSessionAt(i);
                _sessionList.RemoveAt(i);
                --i;
            }
        }

        for (int i = 0; i < newList.Count; i++)
        {
            bool isAdd = true;
            for (int j = 0; j < _sessionList.Count; j++)
            {
                if (newList[i].Name == _sessionList[j].Name)
                {
                    isAdd = false;
                    break;
                }
            }

            if (isAdd)
            {
                _sessionList.Insert(0, newList[i]);
                AddSessionAtTop();
            }
        }

        Settings();

        foreach (AD.Room room in _activeItemsList)
            room.SetRoom(_sessionList[room._sessionIndex], room._sessionIndex);
    }

    /// <summary>
    /// 제거해야 할 List를 받고 현재 보이는 Item을 기준으로 제거
    /// 현재 보이는 Item의 뒷 부분이라면 무시 -> 마지막에 List 덮어 씌움
    /// </summary>
    public void RemoveSessionAt(int removeIndex)
    {
        if (removeIndex < _activeItemFirstIndex)
        {
            foreach (AD.Room item in _activeItemsList)
                MoveActiveItemAfterRemoveItem(item);
        }
        else if (removeIndex <= _activeItemLastIndex)
        {
            AD.Room removedItem = FindItemByIndex(removeIndex);
            _activeItemsList.Remove(removedItem);

            removedItem.gameObject.SetActive(false);
            _inActiveItemsList.AddLast(removedItem);

            foreach (AD.Room item in _activeItemsList)
            {
                int oldIndex = item._sessionIndex;

                if (oldIndex > removeIndex)
                    MoveActiveItemAfterRemoveItem(item);
            }
        }

        if (_activeItemsList.Count > 0)
        {
            _activeItemFirstIndex = _activeItemsList.First.Value._sessionIndex;
            _activeItemLastIndex = _activeItemsList.Last.Value._sessionIndex;
        }
        else
        {
            _activeItemFirstIndex = 0;
            _activeItemLastIndex = 0;
        }
    }

    /// <summary>
    /// 아이템이 추가될 경우 항상 가장 첫번째로(index => 0) 삽입 후 
    /// 기존 Item들의 Index를 ++
    /// </summary>
    public void AddSessionAtTop()
    {
        foreach (AD.Room room in _activeItemsList)
            MoveActiveItemBeforeAddItem(room);

        if (_activeItemFirstIndex == 0)
        {
            if (_inActiveItemsList.Count > 0)
            {
                AD.Room newItem = _inActiveItemsList.First.Value;
                _inActiveItemsList.RemoveFirst();

                newItem.SetRoom(_sessionList[0], 0);
                newItem.gameObject.SetActive(true);

                newItem._thisRect.anchoredPosition =
                    new Vector2(_anchorXList[0], -_contentGridLayoutGroup.padding.top);

                _activeItemsList.AddFirst(newItem);
            }
            else
            {
                AD.Room newItem = _activeItemsList.Last.Value;
                _activeItemsList.RemoveLast();

                newItem.SetRoom(_sessionList[0], 0);

                newItem._thisRect.anchoredPosition =
                    new Vector2(_anchorXList[0], -_contentGridLayoutGroup.padding.top);

                _activeItemsList.AddFirst(newItem);
            }
        }

        _activeItemFirstIndex = _activeItemsList.First.Value._sessionIndex;
        _activeItemLastIndex = _activeItemsList.Last.Value._sessionIndex;
    }

    private AD.Room FindItemByIndex(int index)
    {
        foreach (AD.Room item in _activeItemsList)
        {
            if (item._sessionIndex == index)
                return item;
        }

        return null;
    }

    private void MoveActiveItemAfterRemoveItem(AD.Room item)
    {
        item._sessionIndex = item._sessionIndex - 1;

        for (int j = 0; j < _anchorXList.Count; j++)
        {
            if (item._thisRect.anchoredPosition.x == _anchorXList[j])
            {
                if (j == 0)
                {
                    item._thisRect.anchoredPosition =
                        new Vector2(_anchorXList[_anchorXList.Count - 1], item._thisRect.anchoredPosition.y + _intervalHeight);

                    break;
                }
                else
                {
                    item._thisRect.anchoredPosition =
                        new Vector2(_anchorXList[j - 1], item._thisRect.anchoredPosition.y);

                    break;
                }
            }
        }
    }

    private void MoveActiveItemBeforeAddItem(AD.Room item)
    {
        item._sessionIndex = item._sessionIndex + 1;

        for (int j = -1; j < _anchorXList.Count; j++)
        {
            if (item._thisRect.anchoredPosition.x == _anchorXList[j])
            {
                if (j == _anchorXList.Count - 1)
                {
                    item._thisRect.anchoredPosition =
                        new Vector2(_anchorXList[0], item._thisRect.anchoredPosition.y - _intervalHeight);

                    break;
                }
                else
                {
                    item._thisRect.anchoredPosition =
                        new Vector2(_anchorXList[j + 1], item._thisRect.anchoredPosition.y);

                    break;
                }
            }
        }
    }

    #endregion

    #endregion
}