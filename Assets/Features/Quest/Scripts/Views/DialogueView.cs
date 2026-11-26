using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Features.UI;

namespace Game.Features.Quest.Views
{
    /// <summary>
    /// The one shared dialogue window. Shows lines one by one; on the last line it shows the
    /// choice buttons (if any). Put this on an object that is always active, not on the panel it shows.
    /// </summary>
    public class DialogueView : MonoBehaviour
    {
        private const string NextText = "Tiếp";
        private const string CloseText = "Đóng";
        private const string EmptyLine = "...";

        public static DialogueView Instance { get; private set; }

        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _speakerText;
        [SerializeField] private TMP_Text _lineText;

        [Header("Buttons")]
        [SerializeField] private Button _nextButton;
        [SerializeField] private TMP_Text _nextLabel;
        [SerializeField] private Button _primaryButton;
        [SerializeField] private TMP_Text _primaryLabel;
        [SerializeField] private Button _secondaryButton;
        [SerializeField] private TMP_Text _secondaryLabel;

        private object _owner;
        private IReadOnlyList<string> _lines;
        private int _lineIndex;
        private Action _onPrimary;
        private Action _onSecondary;
        private bool _hasPrimary;
        private bool _hasSecondary;

        private void Awake()
        {
            Instance = this;

            _nextButton.onClick.AddListener(OnNextClicked);
            _primaryButton.onClick.AddListener(OnPrimaryClicked);
            _secondaryButton.onClick.AddListener(OnSecondaryClicked);

            _panel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public bool IsOpenFor(object owner)
        {
            return _panel.activeSelf && _owner == owner;
        }

        /// <summary>
        /// Opens the dialogue. A primary (and optionally a secondary) button is shown after the last line.
        /// The button callbacks decide whether to close the dialogue.
        /// </summary>
        public void Show(object owner, string speaker, IReadOnlyList<string> lines,
            string primaryText = null, Action onPrimary = null,
            string secondaryText = null, Action onSecondary = null)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.CloseAll();

            _owner = owner;
            _lines = lines != null && lines.Count > 0 ? lines : new[] { EmptyLine };
            _lineIndex = 0;
            _onPrimary = onPrimary;
            _onSecondary = onSecondary;
            _hasPrimary = !string.IsNullOrEmpty(primaryText);
            _hasSecondary = _hasPrimary && !string.IsNullOrEmpty(secondaryText);

            if (_speakerText != null)
                _speakerText.text = speaker;

            if (_primaryLabel != null)
                _primaryLabel.text = primaryText;

            if (_secondaryLabel != null)
                _secondaryLabel.text = secondaryText;

            _panel.SetActive(true);
            ShowCurrentLine();
        }

        public void Close()
        {
            _panel.SetActive(false);
            _owner = null;
            _onPrimary = null;
            _onSecondary = null;
        }

        private void ShowCurrentLine()
        {
            bool isLast = _lineIndex >= _lines.Count - 1;
            bool showChoices = isLast && _hasPrimary;

            _lineText.text = _lines[_lineIndex];

            _nextButton.gameObject.SetActive(!showChoices);
            _primaryButton.gameObject.SetActive(showChoices);
            _secondaryButton.gameObject.SetActive(showChoices && _hasSecondary);

            if (_nextLabel != null)
                _nextLabel.text = isLast ? CloseText : NextText;
        }

        private void OnNextClicked()
        {
            if (_lineIndex < _lines.Count - 1)
            {
                _lineIndex++;
                ShowCurrentLine();
                return;
            }

            Close();
        }

        private void OnPrimaryClicked()
        {
            Action action = _onPrimary;
            action?.Invoke();
        }

        private void OnSecondaryClicked()
        {
            Action action = _onSecondary;

            if (action != null)
                action.Invoke();
            else
                Close();
        }
    }
}
