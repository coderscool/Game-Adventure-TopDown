using System;
using UnityEngine;
using Game.Features.Items;

namespace Game.Features.Quest.Data
{
    [Serializable]
    public class QuestItemReward
    {
        [SerializeField] private ItemData _item;

        [Min(1)]
        [SerializeField] private int _amount = 1;

        public ItemData Item => _item;
        public int Amount => Mathf.Max(1, _amount);
    }

    /// <summary>What the player gets for finishing a quest.</summary>
    [Serializable]
    public class QuestReward
    {
        [Min(0)]
        [SerializeField] private int _gold;

        [Min(0)]
        [SerializeField] private int _exp;

        [SerializeField] private QuestItemReward[] _items = new QuestItemReward[0];

        public int Gold => _gold;
        public int Exp => _exp;
        public QuestItemReward[] Items => _items;
    }
}
