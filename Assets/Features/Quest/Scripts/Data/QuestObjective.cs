using System;
using UnityEngine;
using Game.Features.Items;

namespace Game.Features.Quest.Data
{
    /// <summary>One goal of a quest, e.g. "collect 3 herbs" or "talk to the elder".</summary>
    [Serializable]
    public class QuestObjective
    {
        [SerializeField] private QuestObjectiveType _type;

        [Tooltip("CollectItem: the item to collect. Other types: leave empty.")]
        [SerializeField] private ItemData _item;

        [Tooltip("TalkToNpc: the NPC id. ReachMap: the scene name.")]
        [SerializeField] private string _targetId;

        [Min(1)]
        [SerializeField] private int _amount = 1;

        [Tooltip("Text shown in the quest log. Leave empty to generate one.")]
        [SerializeField] private string _description;

        [Tooltip("CollectItem: take the items out of the inventory when the quest is turned in.")]
        [SerializeField] private bool _consumeOnTurnIn = true;

        public QuestObjectiveType Type => _type;
        public ItemData Item => _item;
        public string TargetId => _targetId;
        public int Amount => Mathf.Max(1, _amount);
        public bool ConsumeOnTurnIn => _consumeOnTurnIn;

        public string Describe()
        {
            if (!string.IsNullOrEmpty(_description))
                return _description;

            switch (_type)
            {
                case QuestObjectiveType.CollectItem:
                    return "Thu thập " + (_item != null ? _item.ItemName : "?");
                case QuestObjectiveType.TalkToNpc:
                    return "Nói chuyện với " + _targetId;
                case QuestObjectiveType.ReachMap:
                    return "Đến " + _targetId;
                default:
                    return string.Empty;
            }
        }
    }
}
