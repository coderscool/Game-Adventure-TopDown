public static class InventorySaveDataMapper
{
    public static InventoryItemData[] ToSaveData(Slot[] slots, int fallbackSlotCount)
    {
        int length = (slots != null && slots.Length > 0) ? slots.Length : fallbackSlotCount;
        var data = new InventoryItemData[length];

        for (int i = 0; i < length; i++)
        {
            Slot slot = (slots != null && i < slots.Length) ? slots[i] : null;
            data[i] = ToSlotData(slot);
        }

        return data;
    }

    private static InventoryItemData ToSlotData(Slot slot)
    {
        if (slot != null && !slot.IsEmpty() && slot.currentItem != null && slot.currentItem.data != null)
        {
            return new InventoryItemData
            {
                itemId = slot.currentItem.data.itemId,
                quantity = slot.amount
            };
        }

        return EmptySlot();
    }

    private static InventoryItemData EmptySlot()
    {
        return new InventoryItemData { itemId = string.Empty, quantity = 0 };
    }
}
