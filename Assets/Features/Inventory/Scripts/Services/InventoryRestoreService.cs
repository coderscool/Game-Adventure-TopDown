using UnityEngine;
using Game.Features.Inventory.Data;
using Game.Features.Inventory.Views;
using Game.Features.Items;

namespace Game.Features.Inventory.Services
{
    public sealed class InventoryRestoreService
    {
        private readonly InventoryItemFactory _itemFactory;

        public InventoryRestoreService(InventoryItemFactory itemFactory)
        {
            _itemFactory = itemFactory;
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

                ItemUI itemUI = _itemFactory.Create(itemData, entry.quantity);
                if (itemUI == null)
                    continue;

                slot.SetItem(itemUI, entry.quantity);
            }
        }
    }
}
