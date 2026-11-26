using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using Game.Features.Quest.Data;
using Game.Features.Quest.Systems;

namespace Game.Features.Quest.Views
{
    /// <summary>
    /// The quest log window. Put it on the log panel; UIManager.ToggleQuest opens and closes the panel.
    /// It rebuilds its text every time it opens and whenever quests change while it is open.
    /// </summary>
    public class QuestLogView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _content;

        private QuestManager _quests;

        private void OnEnable()
        {
            _quests = QuestManager.Instance;
            if (_quests != null)
                _quests.Changed += Refresh;

            Refresh();
        }

        private void OnDisable()
        {
            if (_quests != null)
                _quests.Changed -= Refresh;

            _quests = null;
        }

        private void Refresh()
        {
            if (_content == null)
                return;

            if (_quests == null)
            {
                _content.text = string.Empty;
                return;
            }

            var text = new StringBuilder();

            AppendSection(text, "Đang thực hiện", _quests.GetOpenQuests(), true);
            AppendSection(text, "Đã hoàn thành", _quests.GetCompletedQuests(), false);

            if (text.Length == 0)
                text.Append("Chưa có nhiệm vụ nào.");

            _content.text = text.ToString();
        }

        private void AppendSection(StringBuilder text, string header, List<QuestData> quests, bool detailed)
        {
            if (quests.Count == 0)
                return;

            if (text.Length > 0)
                text.AppendLine();

            text.AppendLine("<b>" + header + "</b>");

            foreach (QuestData quest in quests)
            {
                text.AppendLine(detailed ? "<b>" + quest.Title + "</b>" : "- " + quest.Title);

                if (!detailed)
                    continue;

                if (!string.IsNullOrEmpty(quest.Description))
                    text.AppendLine(quest.Description);

                for (int i = 0; i < quest.Objectives.Count; i++)
                    text.AppendLine("  - " + _quests.DescribeObjective(quest, i));

                text.AppendLine();
            }
        }
    }
}
