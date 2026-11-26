using Game.Features.Quest.Model;

namespace Game.Features.Quest.Interfaces
{
    /// <summary>Quest progress snapshot port, used by the save system.</summary>
    public interface IQuestState
    {
        QuestProgressData[] CaptureQuests();

        /// <summary>Replaces all progress from a snapshot. A null snapshot means no quests started.</summary>
        void RestoreQuests(QuestProgressData[] data);
    }
}
