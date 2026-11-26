using System;
using Game.Features.Character.Model;
using Game.Features.Inventory.Data;
using Game.Features.Quest.Model;

namespace Game.Features.Save.Data
{
    [Serializable]
    public class GameData
    {
        public PlayerData player;
        public InventoryItemData[] inventory;
        public QuestProgressData[] quests;
    }
}
