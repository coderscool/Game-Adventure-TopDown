using UnityEngine;

public sealed class InventoryMutationService
{
    private readonly InventoryItemFactory itemFactory;

    public InventoryMutationService(InventoryItemFactory itemFactory)
    {
        this.itemFactory = itemFactory;
    }

    public void AddItem(Slot[] slots, ItemData data, int addAmount)
    {
        if (slots == null || data == null || addAmount <= 0)
            return;

        addAmount = FillExistingStacks(slots, data, addAmount);
        if (addAmount > 0)
        {
            addAmount = FillEmptySlots(slots, data, addAmount);
        }

        if (addAmount > 0)
            Debug.Log("Inventory full");
    }

    public void DropItem(ItemUI itemUI, int amount)
    {
        if (itemUI == null || amount <= 0)
            return;

        Slot slot = itemUI.currentSlot;
        if (slot == null)
            return;

        slot.amount -= amount;

        if (slot.amount <= 0)
        {
            Object.Destroy(itemUI.gameObject);
            slot.Clear();
            return;
        }

        itemUI.SetAmount(slot.amount);
    }

    private int FillExistingStacks(Slot[] slots, ItemData data, int remaining)
    {
        foreach (Slot slot in slots)
        {
            if (slot == null || !slot.CanStack(data))
                continue;

            int space = data.maxStack - slot.amount;
            int move = Mathf.Min(space, remaining);
            slot.amount += move;
            slot.currentItem.SetAmount(slot.amount);

            remaining -= move;
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

            int amount = Mathf.Min(remaining, data.maxStack);
            ItemUI ui = itemFactory.Create(data, amount);
            if (ui == null)
                continue;

            slot.SetItem(ui, amount);
            remaining -= amount;

            if (remaining <= 0)
                return 0;
        }

        return remaining;
    }
}
