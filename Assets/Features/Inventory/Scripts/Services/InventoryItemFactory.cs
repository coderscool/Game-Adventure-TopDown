using UnityEngine;
using UnityEngine.UI;
using Game.Features.Inventory.Views;
using Game.Features.Items;

namespace Game.Features.Inventory.Services
{
    public sealed class InventoryItemFactory
    {
        private readonly GameObject _itemPrefab;
        private readonly Transform _inventoryPanel;
        private readonly Canvas _inventoryCanvas;
        private readonly Transform _dragLayer;

        public InventoryItemFactory(GameObject itemPrefab, Transform inventoryPanel, Canvas inventoryCanvas, Transform dragLayer)
        {
            _itemPrefab = itemPrefab;
            _inventoryPanel = inventoryPanel;
            _inventoryCanvas = inventoryCanvas;
            _dragLayer = dragLayer;
        }

        public ItemUI Create(ItemData itemData, int quantity)
        {
            GameObject itemObj = Object.Instantiate(_itemPrefab, _inventoryPanel);
            ItemUI itemUI = itemObj.GetComponent<ItemUI>();

            if (itemUI == null)
            {
                Debug.LogError("ItemUI component missing on inventory item prefab.");
                Object.Destroy(itemObj);
                return null;
            }

            itemUI.Init(itemData, quantity);
            itemUI.SetRootCanvas(_inventoryCanvas);
            itemUI.SetDragLayer(_dragLayer);

            Image image = itemUI.GetComponentInChildren<Image>();
            if (image == null)
            {
                Debug.LogError("Image component missing on inventory item instance.");
                Object.Destroy(itemObj);
                return null;
            }

            image.sprite = itemData.Icon;
            return itemUI;
        }
    }
}
