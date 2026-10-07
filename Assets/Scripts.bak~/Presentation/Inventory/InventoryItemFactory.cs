using UnityEngine;
using UnityEngine.UI;

public sealed class InventoryItemFactory
{
    private readonly GameObject itemPrefab;
    private readonly Transform inventoryPanel;
    private readonly Canvas inventoryCanvas;
    private readonly Transform dragLayer;

    public InventoryItemFactory(GameObject itemPrefab, Transform inventoryPanel, Canvas inventoryCanvas, Transform dragLayer)
    {
        this.itemPrefab = itemPrefab;
        this.inventoryPanel = inventoryPanel;
        this.inventoryCanvas = inventoryCanvas;
        this.dragLayer = dragLayer;
    }

    public ItemUI Create(ItemData itemData, int quantity)
    {
        GameObject itemObj = Object.Instantiate(itemPrefab, inventoryPanel);
        ItemUI itemUI = itemObj.GetComponent<ItemUI>();

        if (itemUI == null)
        {
            Debug.LogError("ItemUI component missing on inventory item prefab.");
            Object.Destroy(itemObj);
            return null;
        }

        itemUI.Init(itemData, quantity);
        itemUI.SetRootCanvas(inventoryCanvas);
        itemUI.SetDragLayer(dragLayer);

        Image image = itemUI.GetComponentInChildren<Image>();
        if (image == null)
        {
            Debug.LogError("Image component missing on inventory item instance.");
            Object.Destroy(itemObj);
            return null;
        }

        image.sprite = itemData.icon;
        return itemUI;
    }
}
