using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Features.Items
{
    [CreateAssetMenu(menuName = "Inventory/Item")]
    public abstract class ItemData : ScriptableObject
    {
        [FormerlySerializedAs("itemId")]
        [SerializeField] private string _itemId;

        [FormerlySerializedAs("itemName")]
        [SerializeField] private string _itemName;

        [FormerlySerializedAs("icon")]
        [SerializeField] private Sprite _icon;

        [FormerlySerializedAs("stackable")]
        [SerializeField] private bool _stackable = true;

        [FormerlySerializedAs("maxStack")]
        [SerializeField] private int _maxStack = 99;

        [FormerlySerializedAs("Type")]
        [SerializeField] private ItemType _type;

        public string ItemId => _itemId;
        public string ItemName => _itemName;
        public Sprite Icon => _icon;
        public bool Stackable => _stackable;
        public int MaxStack => _maxStack;
        public ItemType Type => _type;
    }

    public enum ItemType
    {
        Ingredient,
        Food,
        Herb,
        Medicine,
        Compass,
        Material,
        DevilFruit
    }
}
