using System;
using Game.Features.Character;
using Game.Features.Inventory;

namespace Game.Features.Save
{
    [Serializable]
    public class GameData
    {
        public PlayerData player;
        public InventoryItemData[] inventory;
    }
}
