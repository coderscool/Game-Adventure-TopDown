using UnityEngine;
using Game.Features.Character;
using Game.Features.Inventory;

namespace Game.Features.Save
{
    public sealed class GameLoadService
    {
        private readonly ISaveStorage _storage;

        public GameLoadService(ISaveStorage storage)
        {
            _storage = storage;
        }

        /// <summary>Restores player and inventory from the save file. Returns false when there is no usable save.</summary>
        public bool TryRestoreFromSave(IPlayerState player, IInventoryState inventory)
        {
            if (!_storage.TryLoad(out GameData data))
            {
                Debug.LogWarning("No save file or invalid data; using scene defaults.");
                // Still build the (empty) inventory so a new game can receive items.
                RestoreInventory(inventory, null);
                return false;
            }

            RestorePlayer(player, data.player);
            RestoreInventory(inventory, data.inventory);
            return true;
        }

        private static void RestorePlayer(IPlayerState player, PlayerData playerData)
        {
            if (player == null)
            {
                Debug.LogWarning("LoadGame: player is null.");
                return;
            }

            playerData.ApplyTo(player);
        }

        private static void RestoreInventory(IInventoryState inventory, InventoryItemData[] inventoryData)
        {
            if (inventory == null)
            {
                Debug.LogWarning("LoadGame: inventory is null; inventory not restored.");
                return;
            }

            inventory.RestoreInventory(inventoryData);
        }
    }
}
