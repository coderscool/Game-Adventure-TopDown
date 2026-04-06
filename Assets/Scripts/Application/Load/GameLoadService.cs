using UnityEngine;

public sealed class GameLoadService
{
    public bool TryRestoreFromSave()
    {
        if (!SaveSystem.TryLoad(out GameData data))
        {
            Debug.LogWarning("No save file or invalid data; using scene defaults.");
            return false;
        }

        RestorePlayer(data.player);
        RestoreInventory(data.inventory);
        return true;
    }

    private static void RestorePlayer(PlayerData playerData)
    {
        if (Player.Instance != null)
        {
            Player.Instance.RestoreFromSave(playerData);
            return;
        }

        Debug.LogWarning("LoadGame: Player.Instance is null.");
    }

    private static void RestoreInventory(InventoryItemData[] inventoryData)
    {
        if (InventoryController.Instance != null)
        {
            InventoryController.Instance.RestoreFromSave(inventoryData);
            return;
        }

        Debug.LogWarning("LoadGame: InventoryController.Instance is null; inventory not restored.");
    }
}
