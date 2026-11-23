using System.Collections.Generic;
using UnityEngine;

namespace Game.Features.Shop.Data
{
    /// <summary>One asset per shop: its name and the items it sells. Assigned to a ShopTrigger in a map.</summary>
    [CreateAssetMenu(menuName = "Shop/ShopData")]
    public class ShopData : ScriptableObject
    {
        [SerializeField] private string _shopId;
        [SerializeField] private string _shopName;
        [SerializeField] private List<ShopEntry> _entries = new List<ShopEntry>();

        public string ShopId => _shopId;
        public string ShopName => _shopName;
        public IReadOnlyList<ShopEntry> Entries => _entries;

        private void OnValidate()
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i] == null || _entries[i].Item == null)
                    Debug.LogWarning($"Shop '{name}': entry {i} has no item.", this);
            }
        }
    }
}
