using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int gold;
    public int exp;
    public int exps;

    public float maxHp;
    public float hp;
    public float maxEnergy;
    public float energy;
    public float maxSpirit;
    public float spirit;

    public string currentMapId;
    public bool isOnBoat = false;

    public float[] position;

    public PlayerData(Player player)
    {
        gold = player.gold;
        exp = player.exp;
        exps = player.exps;
        maxHp = player.maxHp;
        hp = player.hp;
        maxEnergy = player.maxEnergy;
        energy = player.energy;
        maxSpirit = player.maxSpirit;
        spirit = player.spirit;
        currentMapId = player.currentMapId;
        isOnBoat = player.isOnBoat;

        position = new float[3];
        position[0] = player.transform.position.x;
        position[1] = player.transform.position.y;
        position[2] = player.transform.position.z;
    }

    public void ApplyTo(Player player)
    {
        if (player == null)
            return;

        player.gold = gold;
        player.exp = exp;
        player.exps = exps;
        player.maxHp = maxHp;
        player.hp = hp;
        player.maxEnergy = maxEnergy;
        player.energy = energy;
        player.maxSpirit = maxSpirit;
        player.spirit = spirit;
        player.currentMapId = currentMapId;
        player.isOnBoat = isOnBoat;

        if (position != null && position.Length >= 3)
            player.transform.position = new Vector3(position[0], position[1], position[2]);
    }
}
