using UnityEngine;
using UnityEngine.Serialization;
using Game.Features.Character;
using Game.Shared;

namespace Game.Features.World
{
    public class ShopTrigger : MonoBehaviour
    {
        private const KeyCode InteractKey = KeyCode.E;
        private const float PausedTimeScale = 0f;
        private const float NormalTimeScale = 1f;

        [FormerlySerializedAs("shopUI")]
        [SerializeField] private GameObject _shopUI;

        private bool _isPlayerNear;

        private void Update()
        {
            if (!_isPlayerNear || !Input.GetKeyDown(InteractKey))
                return;

            bool shouldOpen = !_shopUI.activeSelf;
            _shopUI.SetActive(shouldOpen);
            Time.timeScale = shouldOpen ? PausedTimeScale : NormalTimeScale;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(GameTags.Player))
                _isPlayerNear = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(GameTags.Player))
                _isPlayerNear = false;
        }
    }
}
