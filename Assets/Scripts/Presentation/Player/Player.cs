using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    public int gold = 100;
    public int exp = 0;
    public int exps = 0;

    public float maxHp = 100;
    public float hp = 100;
    public float maxEnergy = 100;
    public float energy = 100;
    public float maxSpirit = 100;
    public float spirit = 100;

    public string currentMapId = "SampleScene";

    public bool isOnBoat = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SavePlayer()
    {
        if (!SaveSystem.TrySave(this))
            Debug.LogWarning("Save did not complete.");
    }

    /// <summary>Applies saved player stats and position only (inventory is handled by InventoryController).</summary>
    public void RestoreFromSave(PlayerData playerData)
    {
        if (playerData != null)
            playerData.ApplyTo(this);
    }
}
