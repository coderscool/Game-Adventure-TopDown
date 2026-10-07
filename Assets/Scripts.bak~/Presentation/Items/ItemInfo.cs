using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfo : MonoBehaviour
{
    public static ItemInfo Instance;

    public TMP_Text nameText;

    ItemUI currentItem;

    void Awake()
    {
        Instance = this;
        Clear();
    }

    public void Show(ItemUI data)
    {
        if (data == null)
        {
            Clear();
            return;
        }

        //icon.enabled = true;
        //icon.sprite = data.icon;
        currentItem = data;
        nameText.text = data.data.itemName;
    }

    public void Use()
    {
        TeleportManager.Instance.Teleport("SampleScene", "Map2");
    }

    public void Clear()
    {
        //icon.enabled = false;
        nameText.text = "";
    }
}
