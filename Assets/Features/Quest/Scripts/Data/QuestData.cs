using System.Collections.Generic;
using UnityEngine;

namespace Game.Features.Quest.Data
{
    /// <summary>
    /// One quest, as data. Put the asset in a Resources/Quests folder so QuestManager finds it.
    /// A quest starts either when an NPC offers it, or automatically when the player enters a map.
    /// </summary>
    [CreateAssetMenu(menuName = "Quest/QuestData")]
    public class QuestData : ScriptableObject
    {
        [Tooltip("Unique and never changed once players can have it: it is used in save files.")]
        [SerializeField] private string _questId;

        [SerializeField] private string _title;

        [TextArea]
        [SerializeField] private string _description;

        [Header("Start")]
        [Tooltip("Scene name. The quest starts by itself when the player enters this map. Leave empty if an NPC offers it.")]
        [SerializeField] private string _startMapId;

        [Header("End")]
        [Tooltip("NPC id to turn the quest in to. Leave empty to finish (and get the reward) as soon as the goals are done.")]
        [SerializeField] private string _turnInNpcId;

        [Header("Goals and reward")]
        [SerializeField] private QuestObjective[] _objectives = new QuestObjective[0];
        [SerializeField] private QuestReward _reward = new QuestReward();

        [Header("Dialogue")]
        [TextArea] [SerializeField] private string[] _offerLines = new string[0];
        [TextArea] [SerializeField] private string[] _inProgressLines = new string[0];
        [TextArea] [SerializeField] private string[] _completeLines = new string[0];

        public string QuestId => _questId;
        public string Title => _title;
        public string Description => _description;
        public string StartMapId => _startMapId;
        public string TurnInNpcId => _turnInNpcId;
        public IReadOnlyList<QuestObjective> Objectives => _objectives;
        public QuestReward Reward => _reward;
        public IReadOnlyList<string> OfferLines => _offerLines;
        public IReadOnlyList<string> InProgressLines => _inProgressLines;
        public IReadOnlyList<string> CompleteLines => _completeLines;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_questId))
                Debug.LogWarning($"Quest '{name}' has no Quest Id.", this);

            if (_objectives == null || _objectives.Length == 0)
                Debug.LogWarning($"Quest '{name}' has no objectives.", this);
        }
    }
}
