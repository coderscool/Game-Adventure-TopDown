using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Features.Map.Views
{
    /// <summary>
    /// One clickable destination on the map. Place it on the map image where the location is, and
    /// fill in the scene + spawn point the player travels to.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class MapPointView : MonoBehaviour
    {
        [SerializeField] private string _sceneName;
        [SerializeField] private string _spawnId = "PlayerStart";

        [Header("Optional")]
        [SerializeField] private TMP_Text _label;
        [SerializeField] private string _displayName;
        [Tooltip("Shown only while the player is in this point's map.")]
        [SerializeField] private GameObject _currentMarker;

        private Button _button;

        public string SceneName => _sceneName;
        public string SpawnId => _spawnId;
        public Button Button => _button != null ? _button : (_button = GetComponent<Button>());

        private void Awake()
        {
            if (_label != null && !string.IsNullOrEmpty(_displayName))
                _label.text = _displayName;
        }

        /// <summary>Marks this point as "you are here": shows the marker and blocks travelling to it.</summary>
        public void SetCurrent(bool isCurrent)
        {
            if (_currentMarker != null)
                _currentMarker.SetActive(isCurrent);

            Button.interactable = !isCurrent;
        }
    }
}
