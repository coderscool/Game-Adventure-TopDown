using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item")]
public abstract class ItemData : ScriptableObject
{
    public string itemId;
    public string itemName;
    public Sprite icon;
    public bool stackable = true;
    public int maxStack = 99;
    public ItemType Type;
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

