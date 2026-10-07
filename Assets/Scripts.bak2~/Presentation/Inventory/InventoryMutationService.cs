using UnityEngine;

public sealed class InventoryMutationService
{
    private readonly InventoryItemFactory _itemFactory;

    public InventoryMutationService(InventoryItemFactory itemFactory)
    {
        _itemFactory = itemFactory;
    }

    public void AddItem(Slot[] slots, ItemData data, int addAmount)
    {
        if (slots == null || data == null || addAmount <= 0)
            return;

        int remaining = FillExistingStacks(slots, data, addAmount);
        if (remaining > 0)
            remaining = FillEmptySlots(slots, data, remaining);

        if (remaining > 0)
            Debug.Log("Inventory full");
    }

    public void DropItem(ItemUI itemUI, int amount)
    {
        if (itemUI == null || amount <= 0)
            return;

        Slot slot = itemUI.CurrentSlot;
        if (slot == null)
            return;

        slot.Amount -= amount;

        if (slot.Amount <= 0)
        {
            Object.Destroy(itemUI.gameObject);
            slot.Clear();
            return;
        }

        itemUI.SetAmount(slot.Amount);
    }

    private int FillExistingStacks(Slot[] slots, ItemData data, int remaining)
    {
        foreach (Slot slot in slots)
        {
            if (slot == null || !slot.CanStack(data))
                continue;

            int space = data.MaxStack - slot.Amount;
            int moved = Mathf.Min(space, remaining);
            slot.Amount += moved;
            slot.CurrentItem.SetAmount(slot.Amount);

            remaining -= moved;
            if (remaining <= 0)
                return 0;
        }

        return remaining;
    }

    private int FillEmptySlots(Slot[] slots, ItemData data, int remaining)
    {
        foreach (Slot slot in slots)
        {
            if (slot == null || !slot.IsEmpty())
                continue;

            int amount = Mathf.Min(remaining, data.MaxStack);
            ItemUI itemUI = _itemFactory.Create(data, amount);
            if (itemUI == null)
                continue;

            slot.SetItem(itemUI, amount);
            remaining -= amount;

            if (remaining <= 0)
                return 0;
        }

        return remaining;
    }
}
