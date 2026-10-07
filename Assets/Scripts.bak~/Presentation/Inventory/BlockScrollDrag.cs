using UnityEngine;
using UnityEngine.EventSystems;

public class BlockScrollDrag : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public void OnBeginDrag(PointerEventData eventData)
    {
        eventData.pointerDrag = null; // ❌ chặn drag scroll
    }

    public void OnDrag(PointerEventData eventData) { }
}
