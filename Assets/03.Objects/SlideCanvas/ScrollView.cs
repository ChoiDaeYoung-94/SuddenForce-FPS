#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    static ScrollView instance;
    public static ScrollView Instance { get { return instance; } }

    [Header("패널 수, 첫 번째 패널 지정")]
    [SerializeField, Tooltip("패널 개수 지정")] int _panelSize = 0;
    [SerializeField, Tooltip("첫 패널 지정")] short _setFirstPanel = 0;

    [Header("하단 메뉴 버튼 크기 및 고정 여부, Lerp 지정")]
    [SerializeField, Tooltip("현재 패널 버튼의 크기")] float _btnCurrentSize = 0;
    [SerializeField, Tooltip("하단 메뉴 버튼 크기 고정 여부")] bool _isFixedMenuBtnSize = false;
    [SerializeField, Tooltip("하단 메뉴 버튼 Icon Lerp 여부")] bool _isLerpMenuIconSize = false;
    [SerializeField, Tooltip("하단 메뉴 버튼 Icon, Text 둘 다 Lerp 할지")] bool _isLerpBothOfThem = false;

    [Header("하단 메뉴 버튼의 아이콘 lerp Y, scale")]
    [SerializeField, Tooltip("현재 패널 버튼의 아이콘 y축 올릴 정도")] float _btnIconLerpHeight = 0;
    [Tooltip("나머지 패널 버튼의 아이콘 Position")] Vector3 _btnIconPos = Vector3.zero;
    [SerializeField, Tooltip("현재 패널 버튼의 아이콘 y축 올릴때 스케일")] float _btnIconLerpScale = 0;

    [Header("버튼, 아이콘 RectTransform 미리 연동해야 함")]
    [Tooltip("가로로 스크롤 할때의 value를 가져오기 위한 Scrollbar")]
    public Scrollbar _scrollbarHorizontal = null;
    [Tooltip("BottomMenu_Slider")] public Slider _tabSlider = null;
    [Tooltip("BottomMenu_Buttons_RectTransform")] public RectTransform[] _btnRect = null;
    [Tooltip("BottomMenu_Button's Icon_RectTransform")] public RectTransform[] _btnIconRect = null;
    [Tooltip("BottomMenu_Button's Child Icon_RectTransform")] RectTransform[] _IconRect = null;
    [Tooltip("BottomMenu_Button's Child Text_GameObject")] GameObject[] _TextGo = null;

    [Header("아래 부턴 참고용 연동 X-----------------------------------------------------------------------------------------------------------------")]
    [SerializeField, Tooltip("각 패널의 scroll Value상의 거리(패널 간격)")] float _panelDis = 0;
    [SerializeField, Tooltip("각 패널의 scroll Value -> Init에서 초기화")] float[] _panelScrollValue = null;

    [SerializeField, Tooltip("현재 패널의 Index - 1")] int _panel_Index = 0;
    [SerializeField, Tooltip("존재하는 패널의 transform")] Transform[] _TR_Contents = null;
    float _panelFirtTouch = 0, _panelNextTarget = 0; // 첫 터치할때의 패널과 endDrag일때의 패널의 scrollvalue

    [SerializeField, Tooltip("나머지 패널 버튼의 크기")] float _btnRestOfSize = 0;

    [Tooltip("Drag 여부")] bool _isDrag = false;

    private void Awake()
    {
        if (instance == null)
        {
            GameObject go = GameObject.Find("SlideCanvas");
            if (go == null)
            {
                // spawnPrefab
            }

            instance = go.GetComponent<ScrollView>();
        }

        Init();
    }
    
    private void OnDestroy()
    {
        instance = null;
    }

    void Init()
    {
        _panelScrollValue = new float[_panelSize];
        _TR_Contents = new Transform[_panelSize];

        Transform content = transform.GetChild(0).GetChild(0).transform;

        // 버튼 크기를 고정할 경우 or 크기가 달라질 경우
        if (_isFixedMenuBtnSize)
        {
            _btnCurrentSize = _btnRestOfSize = 1080f / _panelSize;
            _tabSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(_btnCurrentSize * 2f, _tabSlider.GetComponent<RectTransform>().sizeDelta.y);
        }
        else
        {
            _btnRestOfSize = (1080f - _btnCurrentSize) / (_panelSize - 1);
            _tabSlider.GetComponent<RectTransform>().sizeDelta =
                new Vector2(_btnCurrentSize + _btnRestOfSize - (_btnCurrentSize - _btnRestOfSize), _tabSlider.GetComponent<RectTransform>().sizeDelta.y);
        }

        // 수평 슬라이드 핸들 사이즈 조절
        RectTransform _tabSliderHandle = _tabSlider.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
        _tabSliderHandle.sizeDelta = new Vector2(_btnCurrentSize, _tabSliderHandle.sizeDelta.y);
        _tabSliderHandle.anchoredPosition = new Vector3(_btnCurrentSize / 2f, _tabSliderHandle.anchoredPosition.y);

        /// <summary>
        /// 패널의 수가 4개일 경우 0 , 0.33, 0.66, 1
        /// => 각 패널간의 거리는 -1을 해줘야 0.33 나오는 느낌으로
        /// 그 뒤 각 패널의 scroll value를 거리를 곱해서 대입
        /// </summary>
        _panelDis = 1f / (_panelSize - 1);
        for (int i = -1; ++i < _panelSize;)
        {
            _TR_Contents[i] = content.GetChild(i).transform; // 각 패널 transform 미리 받아둠
            content.GetChild(i).GetComponent<ScrollRect_>().Init(); // 각 패널의 자식 scrollRect Init
            _panelScrollValue[i] = _panelDis * i;
        }

        // 하단 메뉴 버튼의 Icon, Text를 미리 받고, 기본 IconPos 미리 받아옴        
        _IconRect = new RectTransform[_btnIconRect.Length];
        _TextGo = new GameObject[_btnIconRect.Length];
        for (int i = -1; ++i < _btnIconRect.Length;)
        {
            _IconRect[i] = _btnIconRect[i].GetChild(0).GetComponent<RectTransform>();
            _TextGo[i] = _btnIconRect[i].GetChild(1).gameObject;

            // 아이콘, 텍스트를 둘 다 Lerp할 경우 Text를 미리 끄고, Icon도 살짝 내림
            if (_isLerpBothOfThem)
            {
                _TextGo[i].SetActive(false);
                _IconRect[i].anchoredPosition3D = new Vector3(_IconRect[i].anchoredPosition3D.x, _IconRect[i].anchoredPosition3D.y - 20f, _IconRect[i].anchoredPosition3D.z);
            }
        }

        _btnIconPos = _IconRect[0].anchoredPosition3D;

        // 첫 패널 지정
        BottomPanelBtnClick(_setFirstPanel - 1);
    }

    private void FixedUpdate()
    {
        // 수평 스크롤 시 하단 메뉴의 slider의 value도 변경
        _tabSlider.value = _scrollbarHorizontal.value;

        // 하단 메뉴 버튼 아이콘 및 텍스트 lerp
        LerpBottomMenu(_isLerpMenuIconSize, _isLerpBothOfThem);

        // 드래그 중이 아닐 경우에만(드래그 하고 마우스를 땔 경우) 패널의 반 이상을 움직였을 때 다음 패널로 Lerp 하도록
        if (_isDrag) return;
        else
        {
            _scrollbarHorizontal.value = Mathf.Lerp(_scrollbarHorizontal.value, _panelNextTarget, 0.2f);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그가 시작할 때의 패널 value 받아옴
        _panelFirtTouch = GetscrollbarValue();
    }

    public void OnDrag(PointerEventData eventData)
    {
        _isDrag = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDrag = false;
        // 드래그가 끝날 때의 패널 value 받아옴
        _panelNextTarget = GetscrollbarValue();

        /// <summary>
        /// eventData.delta.x(드래그 되는 가속도)를 받아와서 빠르게 드래그 할 경우에 
        /// OnBeginDrag때의 value와 OnEndDrag때의 value가 같아도 다음 패널로 드래그 할 수 있도록
        /// (=> 각 패널의 처음과 끝일 경우가 아닐 경우에 다음 패널의 value를 받도록)
        /// </summary>
        if (_panelFirtTouch == _panelNextTarget)
        {
            if (eventData.delta.x > 10f && _panel_Index != 0) _panelNextTarget = _panelScrollValue[--_panel_Index];
            if (eventData.delta.x < -10f && _panel_Index != (_panelSize - 1)) _panelNextTarget = _panelScrollValue[++_panel_Index];
        }

        TabBtnCtl();
        VerticalScrollUp();
    }

    #region Functions
    // Drag 상황에 따른 패널의 scrollvalue값을 받아옴
    float GetscrollbarValue()
    {
        int value = 0;
        for (int i = -1; ++i < _panelSize;)
        {
            float half = _panelDis * 0.5f;

            /// <summary>
            /// 각 패널의 scrollvalue에서 패널간의 거리의 반을 더한 부분과 뺀 부분 사이 면 그 패널의 scrollvalue를 반환
            /// ex) 현재 scrollvalue가 0이고 panel_Dis가 0.3일 경우 0에서는 -0.15 ~ 0.15 가 범위가 된다
            ///     => 0.15를 넘게되면 화면의 반을 넘게 되는 것 즉 다음 패널의 값을 반환
            /// </summary>
            if (_scrollbarHorizontal.value > _panelScrollValue[i] - half && _scrollbarHorizontal.value < _panelScrollValue[i] + half)
            {
                _panel_Index = i;
                value = i;
                break;
            }
        }

        return _panelScrollValue[value];
    }

    /// <summary>
    /// 하단 메뉴의 각 버튼 터치시 그에 맞는 패널로 이동
    /// 하단 메뉴의 버튼에서 직접 호출 (각 버튼의 맞는 패널의 index를 매개변수로)
    /// </summary>
    public void BottomPanelBtnClick(int index)
    {
        _panelFirtTouch = GetscrollbarValue();
        _panel_Index = index;
        _panelNextTarget = _panelScrollValue[index];

        TabBtnCtl();
        VerticalScrollUp();
    }

    // 하단 메뉴의 각 버튼 사이즈 조절
    void TabBtnCtl()
    {
        _btnRect[_panel_Index].sizeDelta = new Vector2(_btnCurrentSize, _btnRect[_panel_Index].sizeDelta.y);
        _btnIconRect[_panel_Index].sizeDelta = new Vector2(_btnCurrentSize, _btnRect[_panel_Index].sizeDelta.y);

        for (int i = -1; ++i < _panelSize;)
            if (i != _panel_Index)
            {
                _btnRect[i].sizeDelta = new Vector2(_btnRestOfSize, _btnRect[i].sizeDelta.y);
                _btnIconRect[i].sizeDelta = new Vector2(_btnRestOfSize, _btnIconRect[i].sizeDelta.y);
            }

        // Icon&Text 위치 변환 안됨 -> UGUI 문제 -> refresh
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_btnIconRect[0].parent);

        //Vector3 BtnTargetPos = _btnRect[i].anchoredPosition3D;
        //_btnIconRect[i].anchoredPosition3D = Vector3.Lerp(_btnIconRect[i].anchoredPosition3D, BtnTargetPos, 0.25f);

        //Vector3 BtnTargetScale = Vector3.one;
        //BtnIconRect[i].localScale = Vector3.Lerp(BtnIconRect[i].localScale, BtnTargetScale, 0.25f);
    }

    // 하단 메뉴 버튼 클릭 시 아이콘 및 텍스트 lerp
    void LerpBottomMenu(bool isWork = false, bool BothOfThem = false)
    {
        if (!isWork)
            return;

        for (int i = -1; ++i < _panelSize;)
        {
            Vector3 upPos = _btnIconPos;
            Vector3 upScale = Vector3.one;

            if (i == _panel_Index)
            {
                upPos.y += _btnIconLerpHeight;
                upScale = new Vector3(_btnIconLerpScale, _btnIconLerpScale, 1f);

                if (BothOfThem)
                    _TextGo[i].SetActive(true);
            }
            else
            {
                if (BothOfThem)
                    _TextGo[i].SetActive(false);
            }

            _IconRect[i].anchoredPosition3D = Vector3.Lerp(_IconRect[i].anchoredPosition3D, upPos, 0.2f);
            _IconRect[i].localScale = Vector3.Lerp(_IconRect[i].localScale, upScale, 0.2f);
        }
    }

    /// <summary>
    /// 현재 패널이 ScrollRect_을 가지고 있을 경우
    /// 다음 패널 진입 시 Scrollbar의 value를 1로 하여 
    /// 스크롤 상태를 초기화 
    /// </summary>
    void VerticalScrollUp()
    {
        if (_TR_Contents[_panel_Index].GetComponent<ScrollRect_>() && _panelFirtTouch != _panelNextTarget)
        {
            _TR_Contents[_panel_Index].GetComponentInChildren<Scrollbar>().value = 1;
        }
    }
    #endregion

#if UNITY_EDITOR
    [CustomEditor(typeof(ScrollView))]
    public class Inspector : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("SlideCanvas의 기본 세팅", UnityEditor.MessageType.Info);

            base.OnInspectorGUI();
        }
    }
#endif
}
