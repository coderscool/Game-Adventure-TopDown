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

        /// <summary>True when at least one stack of this item is in the slots.</summary>
        public bool Contains(Slot[] slots, ItemData data)
        {
            if (slots == null || data == null)
                return false;

            foreach (Slot slot in slots)
            {
                if (slot != null && !slot.IsEmpty() && slot.CurrentItem.Data.ItemId == data.ItemId)
                    return true;
            }

            return false;
        }

        /// <summary>Total amount of this item across all slots.</summary>
        public int Count(Slot[] slots, ItemData data)
        {
            if (slots == null || data == null)
                return 0;

            int total = 0;

            foreach (Slot slot in slots)
            {
                if (slot != null && !slot.IsEmpty() && slot.CurrentItem.Data.ItemId == data.ItemId)
                    total += slot.Amount;
            }

            return total;
        }

        /// <summary>Removes the amount from the slots (last slots first). Returns false, and removes nothing, if there is not enough.</summary>
        public bool Remove(Slot[] slots, ItemData data, int amount)
        {
            if (slots == null || data == null || amount <= 0 || Count(slots, data) < amount)
                return false;

            int remaining = amount;

            for (int i = slots.Length - 1; i >= 0 && remaining > 0; i--)
            {
                Slot slot = slots[i];
                if (slot == null || slot.IsEmpty() || slot.CurrentItem.Data.ItemId != data.ItemId)
                    continue;

                int taken = Mathf.Min(slot.Amount, remaining);
                slot.Amount -= taken;
                remaining -= taken;

                if (slot.Amount <= 0)
                {
                    Object.Destroy(slot.CurrentItem.gameObject);
                    slot.Clear();
                }
                else
                {
                    slot.CurrentItem.SetAmount(slot.Amount);
                }
            }

            return true;
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
