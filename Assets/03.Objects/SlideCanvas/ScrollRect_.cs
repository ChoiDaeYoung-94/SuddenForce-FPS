using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollRect_ : ScrollRect
{
    [Tooltip("자식 스크롤뷰를 컨트롤 할 경우 Horizontal은 부모 스크롤뷰의 역할로")]
    bool _horizontal;

    ScrollView _parentScroll = null;
    ScrollRect _parentScrollRect = null;

    // 부모 스크롤뷰 Init시 같이 실행
    public void Init()
    {
        GameObject Scroll_H = GameObject.Find("SlideCanvas");
        _parentScroll = Scroll_H.GetComponent<ScrollView>();
        _parentScrollRect = Scroll_H.GetComponent<ScrollRect>();
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        /// <summary>
        /// x가 더 크면 수평 이동이 더 크니까 부모 스크롤뷰 컨트롤
        /// y가 더 크면 수직 이동이 더 크니까 자식 스크롤뷰 컨트롤
        /// </summary>
        _horizontal = Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y);

        if (_horizontal)
        {
            _parentScroll.OnBeginDrag(eventData);
            _parentScrollRect.OnBeginDrag(eventData);
        }
        else base.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (_horizontal)
        {
            _parentScroll.OnDrag(eventData);
            _parentScrollRect.OnDrag(eventData);
        }
        else base.OnDrag(eventData);

    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (_horizontal)
        {
            _parentScroll.OnEndDrag(eventData);
            _parentScrollRect.OnEndDrag(eventData);
        }
        else base.OnEndDrag(eventData);
    }
}
