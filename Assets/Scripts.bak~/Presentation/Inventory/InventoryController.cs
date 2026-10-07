using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance;
    public Canvas inventoryCanvas;
    public Transform inventoryPanel;
    public Transform dragLayer;
    public GameObject slotPrefab;
    public GameObject itemUIPrefab;
    public ScrollRect scrollRect;
    public int slotCount = 30;

    public Slot[] slots;

    private InventoryItemFactory itemFactory;
    private InventoryRestoreService restoreService;
    private InventoryMutationService mutationService;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            itemFactory = new InventoryItemFactory(itemUIPrefab, inventoryPanel, inventoryCanvas, dragLayer);
            restoreService = new InventoryRestoreService(itemFactory);
            mutationService = new InventoryMutationService(itemFactory);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public InventoryItemData[] GetSaveData()
    {
        return InventorySaveDataMapper.ToSaveData(slots, slotCount);
    }

    public void CreateSlots()
    {
        slots = new Slot[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            slots[i] = Instantiate(slotPrefab, inventoryPanel)
                        .GetComponent<Slot>();
        }

        Debug.Log("Inventory slots created");
    }

    /// <summary>Rebuilds slots and fills them from a save snapshot.</summary>
    public void RestoreFromSave(InventoryItemData[] inventory)
    {
        CreateSlots();
        if (inventory != null && inventory.Length > 0)
            restoreService.Restore(slots, inventory, ItemDatabase.Instance);
    }

    public void SpawnItemToSlot(InventoryItemData[] data)
    {
        restoreService.Restore(slots, data, ItemDatabase.Instance);
    }

    public void AddItem(ItemData data, int addAmount)
    {
        mutationService.AddItem(slots, data, addAmount);
    }

    public void DropItem(ItemUI itemUI, int amount)
    {
        mutationService.DropItem(itemUI, amount);
    }


    public void OnSaveButton()
    {
        SaveRoutine();
    }

    private void SaveRoutine()
    {
        SaveGameService.TrySaveCurrentPlayer();
    }
}

