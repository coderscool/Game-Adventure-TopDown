using System;
using UnityEngine;
using UnityEngine.Serialization;
using Game.Features.Inventory.Data;
using Game.Features.Inventory.Interfaces;
using Game.Features.Inventory.Services;
using Game.Features.Inventory.Views;
using Game.Features.Items;

namespace Game.Features.Inventory
{
    public class InventoryController : MonoBehaviour, IInventoryState
    {
        public static InventoryController Instance { get; private set; }

        [FormerlySerializedAs("inventoryCanvas")] [SerializeField] private Canvas _inventoryCanvas;
        [FormerlySerializedAs("inventoryPanel")] [SerializeField] private Transform _inventoryPanel;
        [FormerlySerializedAs("dragLayer")] [SerializeField] private Transform _dragLayer;
        [FormerlySerializedAs("slotPrefab")] [SerializeField] private GameObject _slotPrefab;
        [FormerlySerializedAs("itemUIPrefab")] [SerializeField] private GameObject _itemUiPrefab;
        [FormerlySerializedAs("slotCount")] [SerializeField] private int _slotCount = 30;

        /// <summary>Raised whenever items are added to or removed from the inventory.</summary>
        public event Action Changed;

        private Slot[] _slots;
        private InventoryRestoreService _restoreService;
        private InventoryMutationService _mutationService;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            var itemFactory = new InventoryItemFactory(_itemUiPrefab, _inventoryPanel, _inventoryCanvas, _dragLayer);
            _restoreService = new InventoryRestoreService(itemFactory);
            _mutationService = new InventoryMutationService(itemFactory);
        }

        public InventoryItemData[] CaptureInventory()
        {
            return InventorySaveDataMapper.ToSaveData(_slots, _slotCount);
        }

        /// <summary>Rebuilds slots and fills them from a save snapshot.</summary>
        public void RestoreInventory(InventoryItemData[] inventory)
        {
            CreateSlots();

            if (inventory != null && inventory.Length > 0)
                _restoreService.Restore(_slots, inventory, ItemDatabase.Instance);

            Changed?.Invoke();
        }

        /// <summary>Returns false when the item did not fully fit.</summary>
        public bool AddItem(ItemData data, int addAmount)
        {
            bool allFit = _mutationService.AddItem(_slots, data, addAmount);
            Changed?.Invoke();
            return allFit;
        }

        /// <summary>Checks the whole amount would fit, without adding anything.</summary>
        public bool CanAddItem(ItemData data, int amount)
        {
            return _mutationService.CanFit(_slots, data, amount);
        }

        /// <summary>True when the player owns at least one of this item.</summary>
        public bool HasItem(ItemData data)
        {
            return _mutationService.Contains(_slots, data);
        }

        /// <summary>Total amount of this item the player owns.</summary>
        public int CountOf(ItemData data)
        {
            return _mutationService.Count(_slots, data);
        }

        /// <summary>Removes the amount. Returns false, and removes nothing, if the player has less.</summary>
        public bool RemoveItem(ItemData data, int amount)
        {
            bool removed = _mutationService.Remove(_slots, data, amount);
            if (removed)
                Changed?.Invoke();
            return removed;
        }

        public void DropItem(ItemUI itemUI, int amount)
        {
            _mutationService.DropItem(itemUI, amount);
            Changed?.Invoke();
        }

        private void CreateSlots()
        {
            _slots = new Slot[_slotCount];

            for (int i = 0; i < _slotCount; i++)
                _slots[i] = Instantiate(_slotPrefab, _inventoryPanel).GetComponent<Slot>();
        }
    }
}
