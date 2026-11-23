using UnityEngine;
using Game.Features.Inventory.Views;
using Game.Features.Items;

namespace Game.Features.Inventory.Services
{
    public sealed class InventoryMutationService
    {
        private readonly InventoryItemFactory _itemFactory;

        public InventoryMutationService(InventoryItemFactory itemFactory)
        {
            _itemFactory = itemFactory;
        }

        /// <summary>Adds the item. Returns false when it did not all fit (the part that fit is still added).</summary>
        public bool AddItem(Slot[] slots, ItemData data, int addAmount)
        {
            if (slots == null || data == null || addAmount <= 0)
                return false;

            int remaining = FillExistingStacks(slots, data, addAmount);
            if (remaining > 0)
                remaining = FillEmptySlots(slots, data, remaining);

            if (remaining > 0)
            {
                Debug.Log("Inventory full");
                return false;
            }

            return true;
        }

        /// <summary>True when the whole amount would fit, without changing anything.</summary>
        public bool CanFit(Slot[] slots, ItemData data, int amount)
        {
            if (slots == null || data == null || amount <= 0)
                return false;

            int space = 0;

            foreach (Slot slot in slots)
            {
                if (slot == null)
                    continue;

                if (slot.CanStack(data))
                    space += data.MaxStack - slot.Amount;
                else if (slot.IsEmpty())
                    space += data.MaxStack;

                if (space >= amount)
                    return true;
            }

            return false;
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
}
