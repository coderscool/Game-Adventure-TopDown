using UnityEngine;
using UnityEngine.Serialization;
using Game.Features.Inventory;
using Game.Features.Items;
using Game.Features.Save;
using Game.Shared;

namespace Game.Features.Pickups
{
    public class ItemPickup : SaveableBehaviour
    {
        private const KeyCode PickupKey = KeyCode.R;
        private const int PickupAmount = 1;

        [FormerlySerializedAs("itemData")]
        [SerializeField] private ItemData _itemData;

        private bool _isPlayerInRange;
        private bool _collected;

        public override string CaptureState()
        {
            return JsonUtility.ToJson(new LootState { collected = _collected });
        }

        public override void RestoreState(string json)
        {
            if (string.IsNullOrEmpty(json))
                return;

            LootState state = JsonUtility.FromJson<LootState>(json);
            if (state == null)
                return;

            _collected = state.collected;

            if (_collected)
                gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_isPlayerInRange && Input.GetKeyDown(PickupKey))
                Pickup();
        }

        private void Pickup()
        {
            _collected = true;
            gameObject.SetActive(false);

            if (InventoryController.Instance != null)
                InventoryController.Instance.AddItem(_itemData, PickupAmount);

            RequestSceneSave();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(GameTags.Player))
                _isPlayerInRange = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(GameTags.Player))
                _isPlayerInRange = false;
        }
    }
}
