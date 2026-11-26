using System.Text;
using TMPro;
using UnityEngine;
using Game.Features.Quest.Data;
using Game.Features.Quest.Model;
using Game.Features.Quest.Systems;

namespace Game.Features.Quest.Views
{
    /// <summary>HUD text showing the first open quest and its goals. Empty when there is none.</summary>
    public class QuestTrackerView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        private QuestManager _quests;

        private void OnEnable()
        {
            Bind();
        }

        private void Start()
        {
            // The manager may not exist yet at OnEnable (bootstrap order); retry once.
            if (_quests == null)
                Bind();
        }

        private void OnDisable()
        {
            if (_quests != null)
                _quests.Changed -= Refresh;

            _quests = null;
        }

        private void Bind()
        {
            if (_quests != null || QuestManager.Instance == null)
                return;

            _quests = QuestManager.Instance;
            _quests.Changed += Refresh;
            Refresh();
        }

        private void Refresh()
        {
            if (_text == null || _quests == null)
                return;

            var open = _quests.GetOpenQuests();
            if (open.Count == 0)
            {
                _text.text = string.Empty;
                return;
            }

            QuestData quest = open[0];
            var text = new StringBuilder();
            text.AppendLine("<b>" + quest.Title + "</b>");

            if (_quests.GetState(quest) == QuestState.ReadyToTurnIn)
            {
                text.Append(string.IsNullOrEmpty(quest.TurnInNpcId)
                    ? "Hãy dọn túi đồ để nhận thưởng"
                    : "Hãy gặp " + quest.TurnInNpcId + " để nhận thưởng");
            }
            else
            {
                for (int i = 0; i < quest.Objectives.Count; i++)
                    text.AppendLine(_quests.DescribeObjective(quest, i));
            }

            _text.text = text.ToString();
        }
    }
}
