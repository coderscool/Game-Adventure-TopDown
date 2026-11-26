using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Features.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [FormerlySerializedAs("inventoryCanvas")]
        [SerializeField] private GameObject _inventoryCanvas;

        [FormerlySerializedAs("systemCanvas")]
        [SerializeField] private GameObject _systemCanvas;

        [SerializeField] private GameObject _mapCanvas;
        [SerializeField] private GameObject _questCanvas;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            CloseAll();
            DontDestroyOnLoad(gameObject);
        }

        public void CloseAll()
        {
            _inventoryCanvas.SetActive(false);
            _systemCanvas.SetActive(false);

            if (_mapCanvas != null)
                _mapCanvas.SetActive(false);

            if (_questCanvas != null)
                _questCanvas.SetActive(false);
        }

        public void ToggleInventory()
        {
            Toggle(_inventoryCanvas);
        }

        public void ToggleSystem()
        {
            Toggle(_systemCanvas);
        }

        // Bound to the quest button's UnityEvent and the J key - keep the name.
        public void ToggleQuest()
        {
            if (_questCanvas == null)
            {
                Debug.LogWarning("UIManager: Quest Canvas is not assigned.");
                return;
            }

            Toggle(_questCanvas);
        }

        // Bound to the map button's UnityEvent and the M key - keep the name.
        public void ToggleMap()
        {
            if (_mapCanvas == null)
            {
                Debug.LogWarning("UIManager: Map Canvas is not assigned.");
                return;
            }

            Toggle(_mapCanvas);
        }

        private void Toggle(GameObject canvas)
        {
            bool wasActive = canvas.activeSelf;
            CloseAll();
            canvas.SetActive(!wasActive);
        }
    }
}
