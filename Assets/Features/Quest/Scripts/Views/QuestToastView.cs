using System.Collections;
using TMPro;
using UnityEngine;
using Game.Features.Quest.Systems;

namespace Game.Features.Quest.Views
{
    /// <summary>Shows short quest messages ("New quest: ...") for a few seconds.</summary>
    public class QuestToastView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [Min(0.5f)]
        [SerializeField] private float _showSeconds = 3f;

        private QuestManager _quests;
        private Coroutine _hideRoutine;

        private void OnEnable()
        {
            if (_text != null)
                _text.gameObject.SetActive(false);

            Bind();
        }

        private void Start()
        {
            if (_quests == null)
                Bind();
        }

        private void OnDisable()
        {
            if (_quests != null)
                _quests.MessageRaised -= Show;

            _quests = null;
            _hideRoutine = null;
        }

        private void Bind()
        {
            if (_quests != null || QuestManager.Instance == null)
                return;

            _quests = QuestManager.Instance;
            _quests.MessageRaised += Show;
        }

        private void Show(string message)
        {
            if (_text == null)
                return;

            _text.text = message;
            _text.gameObject.SetActive(true);

            if (_hideRoutine != null)
                StopCoroutine(_hideRoutine);

            _hideRoutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(_showSeconds);
            _text.gameObject.SetActive(false);
            _hideRoutine = null;
        }
    }
}
