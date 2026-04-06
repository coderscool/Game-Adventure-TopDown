using UnityEngine;

public sealed class InventoryRestoreService
{
    private readonly InventoryItemFactory itemFactory;

    public InventoryRestoreService(InventoryItemFactory itemFactory)
    {
        this.itemFactory = itemFactory;
    }

    public void Restore(Slot[] slots, InventoryItemData[] data, ItemDatabase itemDatabase)
    {
        if (slots == null || data == null)
            return;

        if (itemDatabase == null)
        {
            Debug.LogWarning("ItemDatabase missing, inventory restore skipped.");
            return;
        }

        for (int i = 0; i < data.Length && i < slots.Length; i++)
        {
            InventoryItemData entry = data[i];
            if (entry == null || string.IsNullOrEmpty(entry.itemId))
                continue;

            Slot slot = slots[i];
            if (slot == null || !slot.IsEmpty())
                continue;

            if (!itemDatabase.TryGetItem(entry.itemId, out ItemData itemData))
            {
                Debug.LogWarning($"Save references unknown item id: {entry.itemId}");
                continue;
            }

            ItemUI itemUI = itemFactory.Create(itemData, entry.quantity);
            if (itemUI == null)
                continue;

            slot.SetItem(itemUI, entry.quantity);
        }
    }
}
