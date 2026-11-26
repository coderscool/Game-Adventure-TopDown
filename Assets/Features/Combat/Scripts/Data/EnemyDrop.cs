using System;
using UnityEngine;
using Game.Features.Items;

namespace Game.Features.Combat.Data
{
    /// <summary>An item an enemy may drop, with its chance (0 to 1).</summary>
    [Serializable]
    public class EnemyDrop
    {
        [SerializeField] private ItemData _item;

        [Range(0f, 1f)]
        [SerializeField] private float _chance = 0.3f;

        [Min(1)]
        [SerializeField] private int _amount = 1;

        public ItemData Item => _item;
        public float Chance => _chance;
        public int Amount => Mathf.Max(1, _amount);
    }
}
