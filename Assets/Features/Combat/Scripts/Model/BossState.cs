using System;

namespace Game.Features.Combat.Model
{
    /// <summary>Saved state of a boss: once defeated it stays defeated.</summary>
    [Serializable]
    public class BossState
    {
        public bool defeated;
    }
}
