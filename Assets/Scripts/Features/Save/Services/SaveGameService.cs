using UnityEngine;
using Game.Features.Character.Model;
using Game.Features.Inventory.Data;
using Game.Features.Inventory.Interfaces;
using Game.Features.Save.Data;
using Game.Features.Save.Interfaces;

namespace Game.Features.Save.Services
{
    public sealed class SaveGameService
    {
        private readonly ISaveStorage _storage;

        public SaveGameService(ISaveStorage storage)
        {
            _storage = storage;
        }

        public bool TrySave(IPlayerState player, IInventoryState inventory)
        {
            if (player == null)
            {
                Debug.LogWarning("SaveGame: player is null.");
                return false;
            }

            var data = new GameData
            {
                player = new PlayerData(player),
                inventory = inventory != null ? inventory.CaptureInventory() : new InventoryItemData[0]
            };

            return _storage.TrySave(data);
        }
    }
}
