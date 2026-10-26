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
        }

        public void ToggleInventory()
        {
            Toggle(_inventoryCanvas);
        }

        public void ToggleSystem()
        {
            Toggle(_systemCanvas);
        }

        private void Toggle(GameObject canvas)
        {
            bool wasActive = canvas.activeSelf;
            CloseAll();
            canvas.SetActive(!wasActive);
        }
    }
}
