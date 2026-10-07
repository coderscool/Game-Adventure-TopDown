namespace Game.Features.Inventory
{
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
            bool hasItem = slot != null && !slot.IsEmpty() && slot.CurrentItem.Data != null;

            return new InventoryItemData
            {
                itemId = hasItem ? slot.CurrentItem.Data.ItemId : string.Empty,
                quantity = hasItem ? slot.Amount : 0
            };
        }
    }
}
