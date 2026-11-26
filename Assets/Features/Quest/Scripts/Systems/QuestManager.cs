using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Features.Character.Systems;
using Game.Features.Inventory;
using Game.Features.Quest.Data;
using Game.Features.Quest.Interfaces;
using Game.Features.Quest.Model;

namespace Game.Features.Quest.Systems
{
    /// <summary>
    /// Owns quest progress. Quests come from QuestData assets in Resources/Quests; progress is
    /// advanced by reports (NPC talked, map entered) and by what is in the inventory.
    /// </summary>
    public class QuestManager : MonoBehaviour, IQuestState
    {
        private const string ResourcesPath = "Quests";

        public static QuestManager Instance { get; private set; }

        /// <summary>Raised whenever any quest's state or progress changes.</summary>
        public event Action Changed;

        /// <summary>Short messages for the player ("New quest: ...").</summary>
        public event Action<string> MessageRaised;

        private readonly Dictionary<string, QuestData> _catalog = new Dictionary<string, QuestData>();
        private readonly Dictionary<string, QuestProgress> _progress = new Dictionary<string, QuestProgress>();

        private InventoryController _inventory;
        private bool _isReady;

        // Found lazily: the inventory may be created after this manager.
        private InventoryController PlayerInventory
        {
            get
            {
                if (_inventory == null && InventoryController.Instance != null)
                {
                    _inventory = InventoryController.Instance;
                    _inventory.Changed += OnInventoryChanged;
                }

                return _inventory;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCatalog();
        }

        private void OnEnable()
        {
            if (Instance != this)
                return;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            if (Instance != this)
                return;

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;

            if (_inventory != null)
                _inventory.Changed -= OnInventoryChanged;

            Instance = null;
        }

        // ---------- Queries ----------

        public QuestState GetState(QuestData quest)
        {
            if (quest == null || !_progress.TryGetValue(quest.QuestId, out QuestProgress progress))
                return QuestState.NotStarted;

            return progress.State;
        }

        /// <summary>Quests that are in progress or waiting to be turned in.</summary>
        public List<QuestData> GetOpenQuests()
        {
            var result = new List<QuestData>();

            foreach (KeyValuePair<string, QuestProgress> pair in _progress)
            {
                bool isOpen = pair.Value.State == QuestState.Active || pair.Value.State == QuestState.ReadyToTurnIn;
                if (isOpen && _catalog.TryGetValue(pair.Key, out QuestData quest))
                    result.Add(quest);
            }

            result.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.Ordinal));
            return result;
        }

        public List<QuestData> GetCompletedQuests()
        {
            var result = new List<QuestData>();

            foreach (KeyValuePair<string, QuestProgress> pair in _progress)
            {
                if (pair.Value.State == QuestState.Completed && _catalog.TryGetValue(pair.Key, out QuestData quest))
                    result.Add(quest);
            }

            result.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.Ordinal));
            return result;
        }

        /// <summary>A quest whose goals are done and which is turned in to this NPC.</summary>
        public QuestData FindReadyToTurnInAt(string npcId)
        {
            if (string.IsNullOrEmpty(npcId))
                return null;

            foreach (QuestData quest in GetOpenQuests())
            {
                if (GetState(quest) == QuestState.ReadyToTurnIn && quest.TurnInNpcId == npcId)
                    return quest;
            }

            return null;
        }

        public int GetCount(QuestData quest, int objectiveIndex)
        {
            QuestObjective objective = quest.Objectives[objectiveIndex];

            if (objective.Type == QuestObjectiveType.CollectItem)
            {
                if (PlayerInventory == null || objective.Item == null)
                    return 0;

                return Mathf.Min(PlayerInventory.CountOf(objective.Item), objective.Amount);
            }

            if (_progress.TryGetValue(quest.QuestId, out QuestProgress progress) && objectiveIndex < progress.Counts.Length)
                return Mathf.Min(progress.Counts[objectiveIndex], objective.Amount);

            return 0;
        }

        public string DescribeObjective(QuestData quest, int objectiveIndex)
        {
            QuestObjective objective = quest.Objectives[objectiveIndex];
            return $"{objective.Describe()} ({GetCount(quest, objectiveIndex)}/{objective.Amount})";
        }

        // ---------- Actions ----------

        public bool Accept(QuestData quest)
        {
            if (quest == null || GetState(quest) != QuestState.NotStarted)
                return false;

            _progress[quest.QuestId] = new QuestProgress(quest.Objectives.Count);
            Raise("Nhiệm vụ mới: " + quest.Title);

            Evaluate(quest);
            Changed?.Invoke();
            return true;
        }

        public TurnInResult TurnIn(QuestData quest)
        {
            if (quest == null || GetState(quest) != QuestState.ReadyToTurnIn || !AreAllObjectivesDone(quest))
                return TurnInResult.NotReady;

            if (!CanReceiveReward(quest))
            {
                Raise("Túi đồ đã đầy");
                return TurnInResult.InventoryFull;
            }

            Complete(quest);
            return TurnInResult.Success;
        }

        /// <summary>The player talked to an NPC.</summary>
        public void ReportNpcTalked(string npcId)
        {
            Report(QuestObjectiveType.TalkToNpc, npcId);
        }

        /// <summary>The player arrived in a map: starts quests tied to it and counts "reach map" goals.</summary>
        public void ReportMapEntered(string mapId)
        {
            if (string.IsNullOrEmpty(mapId))
                return;

            foreach (QuestData quest in new List<QuestData>(_catalog.Values))
            {
                if (quest.StartMapId == mapId && GetState(quest) == QuestState.NotStarted)
                    Accept(quest);
            }

            Report(QuestObjectiveType.ReachMap, mapId);
        }

        // ---------- Save / load ----------

        public QuestProgressData[] CaptureQuests()
        {
            var list = new List<QuestProgressData>();

            foreach (KeyValuePair<string, QuestProgress> pair in _progress)
            {
                list.Add(new QuestProgressData
                {
                    questId = pair.Key,
                    state = (int)pair.Value.State,
                    counts = (int[])pair.Value.Counts.Clone()
                });
            }

            return list.ToArray();
        }

        public void RestoreQuests(QuestProgressData[] data)
        {
            _progress.Clear();

            if (data != null)
            {
                foreach (QuestProgressData entry in data)
                {
                    if (entry == null)
                        continue;

                    if (!_catalog.TryGetValue(entry.questId, out QuestData quest))
                    {
                        Debug.LogWarning($"Saved quest '{entry.questId}' no longer exists; skipped.");
                        continue;
                    }

                    var progress = new QuestProgress(quest.Objectives.Count) { State = (QuestState)entry.state };
                    if (entry.counts != null)
                        Array.Copy(entry.counts, progress.Counts, Mathf.Min(entry.counts.Length, progress.Counts.Length));

                    _progress[quest.QuestId] = progress;
                }
            }

            _isReady = true;
            EvaluateAll();
            ReportMapEntered(SceneManager.GetActiveScene().name);
            Changed?.Invoke();
        }

        // ---------- Internals ----------

        private void LoadCatalog()
        {
            _catalog.Clear();

            foreach (QuestData quest in Resources.LoadAll<QuestData>(ResourcesPath))
            {
                if (quest == null || string.IsNullOrEmpty(quest.QuestId))
                {
                    Debug.LogWarning("A quest asset has no Quest Id; skipped.", quest);
                    continue;
                }

                if (_catalog.ContainsKey(quest.QuestId))
                {
                    Debug.LogWarning($"Duplicate Quest Id '{quest.QuestId}' ({quest.name}); skipped.", quest);
                    continue;
                }

                _catalog.Add(quest.QuestId, quest);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_isReady)
                ReportMapEntered(scene.name);
        }

        private void OnInventoryChanged()
        {
            EvaluateAll();
            Changed?.Invoke();
        }

        private void Report(QuestObjectiveType type, string targetId)
        {
            if (string.IsNullOrEmpty(targetId))
                return;

            bool changed = false;

            foreach (QuestData quest in GetOpenQuests())
            {
                QuestProgress progress = _progress[quest.QuestId];
                if (progress.State != QuestState.Active)
                    continue;

                for (int i = 0; i < quest.Objectives.Count; i++)
                {
                    QuestObjective objective = quest.Objectives[i];
                    if (objective.Type != type || objective.TargetId != targetId || progress.Counts[i] >= objective.Amount)
                        continue;

                    progress.Counts[i]++;
                    changed = true;
                }

                Evaluate(quest);
            }

            if (changed)
                Changed?.Invoke();
        }

        private void EvaluateAll()
        {
            foreach (QuestData quest in GetOpenQuests())
                Evaluate(quest);
        }

        // Moves a quest between Active and ReadyToTurnIn (or finishes it) depending on its goals.
        private void Evaluate(QuestData quest)
        {
            if (!_progress.TryGetValue(quest.QuestId, out QuestProgress progress))
                return;

            if (progress.State != QuestState.Active && progress.State != QuestState.ReadyToTurnIn)
                return;

            bool allDone = AreAllObjectivesDone(quest);

            if (!allDone)
            {
                progress.State = QuestState.Active;
                return;
            }

            bool finishesByItself = string.IsNullOrEmpty(quest.TurnInNpcId);

            if (finishesByItself && CanReceiveReward(quest))
            {
                Complete(quest);
                return;
            }

            if (progress.State == QuestState.Active)
            {
                progress.State = QuestState.ReadyToTurnIn;
                Raise(finishesByItself ? "Túi đồ đầy, hãy dọn chỗ để nhận thưởng" : "Đã xong mục tiêu: " + quest.Title);
            }
        }

        private bool AreAllObjectivesDone(QuestData quest)
        {
            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                if (GetCount(quest, i) < quest.Objectives[i].Amount)
                    return false;
            }

            return true;
        }

        private bool CanReceiveReward(QuestData quest)
        {
            foreach (QuestItemReward reward in quest.Reward.Items)
            {
                if (reward == null || reward.Item == null)
                    continue;

                if (PlayerInventory == null || !PlayerInventory.CanAddItem(reward.Item, reward.Amount))
                    return false;
            }

            return true;
        }

        private void Complete(QuestData quest)
        {
            // Mark it done first: taking items and giving rewards changes the inventory,
            // which re-evaluates quests.
            _progress[quest.QuestId].State = QuestState.Completed;

            if (PlayerInventory != null)
            {
                for (int i = 0; i < quest.Objectives.Count; i++)
                {
                    QuestObjective objective = quest.Objectives[i];
                    if (objective.Type == QuestObjectiveType.CollectItem && objective.ConsumeOnTurnIn && objective.Item != null)
                        PlayerInventory.RemoveItem(objective.Item, objective.Amount);
                }
            }

            GiveReward(quest.Reward);
            Raise("Hoàn thành nhiệm vụ: " + quest.Title);
            Changed?.Invoke();
        }

        private void GiveReward(QuestReward reward)
        {
            Player player = Player.Instance;
            if (player != null)
            {
                player.Gold += reward.Gold;
                player.Exp += reward.Exp;
            }

            if (PlayerInventory == null)
                return;

            foreach (QuestItemReward itemReward in reward.Items)
            {
                if (itemReward != null && itemReward.Item != null)
                    PlayerInventory.AddItem(itemReward.Item, itemReward.Amount);
            }
        }

        private void Raise(string message)
        {
            MessageRaised?.Invoke(message);
        }
    }
}
