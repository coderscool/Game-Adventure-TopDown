using System;
using UnityEngine;

public class ItemPickup : SaveableBehaviour
{
    public ItemData itemData;

    private bool isPlayerInRange = false;

    private bool collected;

    public override object CaptureState()
    {
        return new LootState
        {
            collected = collected
        };
    }

    public override void RestoreState(object state)
    {
        var json = (string)state;

        var data = JsonUtility.FromJson<LootState>(json);

        collected = data.collected;

        if (collected)
            gameObject.SetActive(false);
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.R))
        {
            Pickup();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    void Pickup()
    {
        collected = true;

        gameObject.SetActive(false);

        InventoryController.Instance.AddItem(itemData, 1);

        GameManager.Instance.SaveSceneState(gameObject.scene.name);

        //Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Press E to pick up");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

}
