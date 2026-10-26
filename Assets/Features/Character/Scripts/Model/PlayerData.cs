using UnityEngine;

namespace Game.Features.Character.Model
{
    // Field names are the JSON save format (JsonUtility) - do not rename without a migration.
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
        public bool isOnBoat;

        public float[] position;

        public PlayerData(IPlayerState player)
        {
            gold = player.Gold;
            exp = player.Exp;
            exps = player.Exps;
            maxHp = player.MaxHp;
            hp = player.Hp;
            maxEnergy = player.MaxEnergy;
            energy = player.Energy;
            maxSpirit = player.MaxSpirit;
            spirit = player.Spirit;
            currentMapId = player.CurrentMapId;
            isOnBoat = player.IsOnBoat;

            Vector3 worldPosition = player.Position;
            position = new[] { worldPosition.x, worldPosition.y, worldPosition.z };
        }

        public void ApplyTo(IPlayerState player)
        {
            if (player == null)
                return;

            player.Gold = gold;
            player.Exp = exp;
            player.Exps = exps;
            player.MaxHp = maxHp;
            player.Hp = hp;
            player.MaxEnergy = maxEnergy;
            player.Energy = energy;
            player.MaxSpirit = maxSpirit;
            player.Spirit = spirit;
            player.CurrentMapId = currentMapId;
            player.IsOnBoat = isOnBoat;

            if (position != null && position.Length >= 3)
                player.Position = new Vector3(position[0], position[1], position[2]);
        }
    }
}
