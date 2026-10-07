using UnityEngine;
using UnityEngine.EventSystems;

public class BlockScrollDrag : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Chặn drag để ScrollRect cha không nhận sự kiện kéo.
        eventData.pointerDrag = null;
    }

    public void OnDrag(PointerEventData eventData) { }
}
