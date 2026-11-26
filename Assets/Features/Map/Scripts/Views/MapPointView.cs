using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Features.Items;

namespace Game.Features.Map.Views
{
    /// <summary>
    /// One clickable destination on the map. Place it on the map image where the location is, and
    /// fill in the scene + spawn point the player travels to. Give it a Required Item (e.g. a compass)
    /// and it stays locked until the player owns that item.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class MapPointView : MonoBehaviour
    {
        private const string LockedLabel = "???";

        [SerializeField] private string _sceneName;
        [SerializeField] private string _spawnId = "PlayerStart";

        [Header("Unlock (optional)")]
        [Tooltip("Leave empty for a point that is always available.")]
        [SerializeField] private ItemData _requiredItem;

        [Header("Optional")]
        [SerializeField] private TMP_Text _label;
        [SerializeField] private string _displayName;
        [Tooltip("Shown only while the player is in this point's map.")]
        [SerializeField] private GameObject _currentMarker;

        private Button _button;

        public string SceneName => _sceneName;
        public string SpawnId => _spawnId;
        public ItemData RequiredItem => _requiredItem;
        public Button Button => _button != null ? _button : (_button = GetComponent<Button>());

        /// <summary>
        /// Updates what the player sees: "you are here" blocks travel, locked points hide their name
        /// and cannot be clicked.
        /// </summary>
        public void Refresh(bool isCurrent, bool isUnlocked)
        {
            if (_currentMarker != null)
                _currentMarker.SetActive(isCurrent);

            if (_label != null)
                _label.text = isUnlocked ? _displayName : LockedLabel;

            Button.interactable = isUnlocked && !isCurrent;
        }
    }
}
