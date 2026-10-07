using UnityEngine;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    public List<ItemData> items;

    private Dictionary<string, ItemData> itemDict;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        itemDict = new Dictionary<string, ItemData>();

        foreach (var item in items)
        {
            itemDict[item.itemId] = item;
        }
    }

    public ItemData GetItem(string id)
    {
        return itemDict[id];
    }

    public bool TryGetItem(string id, out ItemData item)
    {
        if (string.IsNullOrEmpty(id) || itemDict == null)
        {
            item = null;
            return false;
        }

        return itemDict.TryGetValue(id, out item);
    }
}