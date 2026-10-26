using UnityEngine;

namespace Game.Features.Character.Model
{
    /// <summary>Player stats that can be saved/restored. Implemented by the Presentation layer.</summary>
    public interface IPlayerState
    {
        int Gold { get; set; }
        int Exp { get; set; }
        int Exps { get; set; }

        float MaxHp { get; set; }
        float Hp { get; set; }
        float MaxEnergy { get; set; }
        float Energy { get; set; }
        float MaxSpirit { get; set; }
        float Spirit { get; set; }

        string CurrentMapId { get; set; }
        bool IsOnBoat { get; set; }
        Vector3 Position { get; set; }
    }
}
