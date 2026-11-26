using UnityEngine;
using Game.Features.Quest.Data;
using Game.Features.Quest.Model;
using Game.Features.Quest.Views;
using Game.Shared;

namespace Game.Features.Quest.Systems
{
    /// <summary>
    /// Put on an NPC (with a trigger collider). Press E near it to talk: it turns in finished quests,
    /// offers its quests, or says what is still to do. Talking also counts for "talk to NPC" goals.
    /// </summary>
    public class NpcInteractable : MonoBehaviour
    {
        private const KeyCode InteractKey = KeyCode.E;

        [Tooltip("Unique id used by quests ('talk to NPC' goals and turn-in).")]
        [SerializeField] private string _npcId;

        [SerializeField] private string _displayName;

        [Tooltip("Quests this NPC offers, in the order they should be offered.")]
        [SerializeField] private QuestData[] _offeredQuests = new QuestData[0];

        [TextArea]
        [SerializeField] private string[] _idleLines = { "..." };

        private bool _isPlayerNear;

        private void Update()
        {
            if (_isPlayerNear && Input.GetKeyDown(InteractKey))
                Interact();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(GameTags.Player))
                _isPlayerNear = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(GameTags.Player))
                return;

            _isPlayerNear = false;
            CloseIfTalkingToThisNpc();
        }

        private void OnDisable()
        {
            _isPlayerNear = false;
            CloseIfTalkingToThisNpc();
        }

        private void Interact()
        {
            DialogueView dialogue = DialogueView.Instance;
            QuestManager quests = QuestManager.Instance;

            if (dialogue == null || quests == null)
            {
                Debug.LogWarning("NpcInteractable: no DialogueView or QuestManager.");
                return;
            }

            if (dialogue.IsOpenFor(this))
            {
                dialogue.Close();
                return;
            }

            quests.ReportNpcTalked(_npcId);

            QuestData toTurnIn = quests.FindReadyToTurnInAt(_npcId);
            if (toTurnIn != null)
            {
                dialogue.Show(this, _displayName, toTurnIn.CompleteLines,
                    "Hoàn thành", () => { quests.TurnIn(toTurnIn); dialogue.Close(); });
                return;
            }

            QuestData toOffer = FindOffered(quests, QuestState.NotStarted);
            if (toOffer != null)
            {
                dialogue.Show(this, _displayName, toOffer.OfferLines,
                    "Nhận nhiệm vụ", () => { quests.Accept(toOffer); dialogue.Close(); },
                    "Để sau", dialogue.Close);
                return;
            }

            QuestData inProgress = FindOffered(quests, QuestState.Active) ?? FindOffered(quests, QuestState.ReadyToTurnIn);
            if (inProgress != null && inProgress.InProgressLines.Count > 0)
            {
                dialogue.Show(this, _displayName, inProgress.InProgressLines);
                return;
            }

            dialogue.Show(this, _displayName, _idleLines);
        }

        private QuestData FindOffered(QuestManager quests, QuestState state)
        {
            foreach (QuestData quest in _offeredQuests)
            {
                if (quest != null && quests.GetState(quest) == state)
                    return quest;
            }

            return null;
        }

        private void CloseIfTalkingToThisNpc()
        {
            DialogueView dialogue = DialogueView.Instance;
            if (dialogue != null && dialogue.IsOpenFor(this))
                dialogue.Close();
        }
    }
}
