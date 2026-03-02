using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectForwarder : MonoBehaviour, IScrollHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ScrollRect _parentScrollRect;

    private void Awake()
    {
        _parentScrollRect = GetComponentInParent<ScrollRect>();
    }

    public void OnScroll(PointerEventData eventData) => _parentScrollRect?.OnScroll(eventData);
    public void OnBeginDrag(PointerEventData eventData) => _parentScrollRect?.OnBeginDrag(eventData);
    public void OnDrag(PointerEventData eventData) => _parentScrollRect?.OnDrag(eventData);
    public void OnEndDrag(PointerEventData eventData) => _parentScrollRect?.OnEndDrag(eventData);
}