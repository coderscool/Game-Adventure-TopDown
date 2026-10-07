using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Features.Inventory
{
    public class BlockScrollDrag : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        public void OnBeginDrag(PointerEventData eventData)
        {
            // Chặn drag để ScrollRect cha không nhận sự kiện kéo.
            eventData.pointerDrag = null;
        }

        public void OnDrag(PointerEventData eventData) { }
    }
}
