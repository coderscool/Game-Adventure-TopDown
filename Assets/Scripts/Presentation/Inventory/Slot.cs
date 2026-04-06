using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler
{
    public ItemUI currentItem;
    public int amount;

    public bool IsEmpty()
    {
        return currentItem == null;
    }

    public bool CanStack(ItemData data)
    {
        return !IsEmpty()
            && currentItem.data.itemId == data.itemId
            && currentItem.data.stackable
            && amount < currentItem.data.maxStack;
    }

    public void SetItem(ItemUI item, int amount)
    {
        Debug.Log(amount);
        currentItem = item;
        this.amount = amount;

        item.currentSlot = this;

        item.transform.SetParent(transform, false);
        item.transform.localPosition = Vector3.zero;

        item.SetAmount(amount);
    }

    public void Clear()
    {
        currentItem = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        ItemUI dragged = eventData.pointerDrag?.GetComponent<ItemUI>();
        if (dragged == null) return;

        Slot from = dragged.currentSlot;
        if (from == this) return;

        if (CanStack(dragged.data))
        {
            int space = dragged.data.maxStack - amount;
            int move = Mathf.Min(space, from.amount);

            amount += move;
            from.amount -= move;

            currentItem.SetAmount(amount);
            dragged.SetAmount(from.amount);

            if (from.amount <= 0)
            {
                Destroy(dragged.gameObject);
                from.Clear();
            }
            return;
        }

        ItemUI tempItem = currentItem;
        int tempAmount = amount;

        from.Clear();
        SetItem(dragged, dragged.currentSlot.amount);

        if (tempItem != null)
            from.SetItem(tempItem, tempAmount);
    }
}

