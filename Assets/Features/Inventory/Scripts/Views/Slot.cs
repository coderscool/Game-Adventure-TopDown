using UnityEngine;
using UnityEngine.EventSystems;
using Game.Features.Items;

namespace Game.Features.Inventory.Views
{
    public class Slot : MonoBehaviour, IDropHandler
    {
        public ItemUI CurrentItem { get; private set; }
        public int Amount { get; set; }

        public bool IsEmpty()
        {
            return CurrentItem == null;
        }

        public bool CanStack(ItemData data)
        {
            return !IsEmpty()
                && CurrentItem.Data.ItemId == data.ItemId
                && CurrentItem.Data.Stackable
                && Amount < CurrentItem.Data.MaxStack;
        }

        public void SetItem(ItemUI item, int quantity)
        {
            CurrentItem = item;
            Amount = quantity;

            item.CurrentSlot = this;

            item.transform.SetParent(transform, false);
            item.transform.localPosition = Vector3.zero;

            item.SetAmount(quantity);
        }

        public void Clear()
        {
            CurrentItem = null;
            Amount = 0;
        }

        public void OnDrop(PointerEventData eventData)
        {
            ItemUI dragged = eventData.pointerDrag != null
                ? eventData.pointerDrag.GetComponent<ItemUI>()
                : null;
            if (dragged == null)
                return;

            Slot from = dragged.CurrentSlot;
            if (from == null || from == this)
                return;

            if (CanStack(dragged.Data))
                MergeFrom(from, dragged);
            else
                SwapWith(from, dragged);
        }

        private void MergeFrom(Slot from, ItemUI dragged)
        {
            int space = dragged.Data.MaxStack - Amount;
            int moved = Mathf.Min(space, from.Amount);

            Amount += moved;
            from.Amount -= moved;

            CurrentItem.SetAmount(Amount);
            dragged.SetAmount(from.Amount);

            if (from.Amount > 0)
                return;

            Destroy(dragged.gameObject);
            from.Clear();
        }

        private void SwapWith(Slot from, ItemUI dragged)
        {
            ItemUI displacedItem = CurrentItem;
            int displacedAmount = Amount;
            int draggedAmount = from.Amount;

            from.Clear();
            SetItem(dragged, draggedAmount);

            if (displacedItem != null)
                from.SetItem(displacedItem, displacedAmount);
        }
    }
}
