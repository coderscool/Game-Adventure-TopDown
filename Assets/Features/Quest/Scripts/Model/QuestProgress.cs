namespace Game.Features.Quest.Model
{
    /// <summary>Runtime progress of one quest the player has started.</summary>
    public sealed class QuestProgress
    {
        public QuestState State;

        /// <summary>One counter per objective (collect objectives are counted from the inventory instead).</summary>
        public readonly int[] Counts;

        public QuestProgress(int objectiveCount)
        {
            State = QuestState.Active;
            Counts = new int[objectiveCount];
        }
    }
}
