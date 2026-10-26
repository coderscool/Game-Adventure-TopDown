using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Features.Items
{
    public class ItemDatabase : MonoBehaviour
    {
        public static ItemDatabase Instance { get; private set; }

        [FormerlySerializedAs("items")]
        [SerializeField] private List<ItemData> _items = new List<ItemData>();

        private readonly Dictionary<string, ItemData> _itemsById = new Dictionary<string, ItemData>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            BuildLookup();
        }

        public bool TryGetItem(string id, out ItemData item)
        {
            item = null;
            return !string.IsNullOrEmpty(id) && _itemsById.TryGetValue(id, out item);
        }

        private void BuildLookup()
        {
            foreach (ItemData item in _items)
            {
                if (item == null || string.IsNullOrEmpty(item.ItemId))
                    continue;

                _itemsById[item.ItemId] = item;
            }
        }
    }
}
