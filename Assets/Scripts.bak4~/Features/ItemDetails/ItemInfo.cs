using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Game.Features.Inventory;
using Game.Features.World;

namespace Game.Features.ItemDetails
{
    public class ItemInfo : MonoBehaviour
    {
        private const string UseTargetScene = "SampleScene";
        private const string UseTargetSpawnId = "Map2";

        [FormerlySerializedAs("nameText")]
        [SerializeField] private TMP_Text _nameText;

        private void Awake()
        {
            ItemUI.Clicked += Show;
            Clear();
        }

        private void OnDestroy()
        {
            ItemUI.Clicked -= Show;
        }

        public void Show(ItemUI itemUI)
        {
            if (itemUI == null || itemUI.Data == null)
            {
                Clear();
                return;
            }

            _nameText.text = itemUI.Data.ItemName;
        }

        // Bound to a UnityEvent in the GameStart scene - keep the name.
        public void Use()
        {
            if (TeleportManager.Instance != null)
                TeleportManager.Instance.Teleport(UseTargetScene, UseTargetSpawnId);
        }

        public void Clear()
        {
            _nameText.text = string.Empty;
        }
    }
}
