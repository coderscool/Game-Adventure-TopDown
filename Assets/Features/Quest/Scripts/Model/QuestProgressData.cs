using System;

namespace Game.Features.Quest.Model
{
    // Field names are the JSON save format (JsonUtility) - do not rename without a migration.
    [Serializable]
    public class QuestProgressData
    {
        public string questId;
        public int state;
        public int[] counts;
    }
}
