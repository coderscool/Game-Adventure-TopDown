using UnityEngine;
using UnityEngine.Serialization;
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
        }

        public void AddItem(ItemData data, int addAmount)
        {
            _mutationService.AddItem(_slots, data, addAmount);
        }

        public void DropItem(ItemUI itemUI, int amount)
        {
            _mutationService.DropItem(itemUI, amount);
        }

        private void CreateSlots()
        {
            _slots = new Slot[_slotCount];

            for (int i = 0; i < _slotCount; i++)
                _slots[i] = Instantiate(_slotPrefab, _inventoryPanel).GetComponent<Slot>();
        }
    }
}
