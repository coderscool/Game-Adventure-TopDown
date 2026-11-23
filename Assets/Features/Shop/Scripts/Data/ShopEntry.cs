using System;
using UnityEngine;
using Game.Features.Items;

namespace Game.Features.Shop.Data
{
    /// <summary>One line of a shop: what it sells and the price in THIS shop.</summary>
    [Serializable]
    public class ShopEntry
    {
        [SerializeField] private ItemData _item;

        [Min(0)]
        [SerializeField] private int _price;

        public ItemData Item => _item;
        public int Price => _price;
    }
}
