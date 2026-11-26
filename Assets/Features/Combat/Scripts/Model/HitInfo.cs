using UnityEngine;

namespace Game.Features.Combat.Model
{
    /// <summary>One hit: how much damage, which way it pushes, and who dealt it.</summary>
    public readonly struct HitInfo
    {
        public readonly float Damage;

        /// <summary>Unit vector pointing from the attacker towards the target.</summary>
        public readonly Vector2 Direction;

        /// <summary>Knockback speed given to the target for a short moment.</summary>
        public readonly float Knockback;

        public readonly DamageTeam SourceTeam;

        public HitInfo(float damage, Vector2 direction, float knockback, DamageTeam sourceTeam)
        {
            Damage = damage;
            Direction = direction;
            Knockback = knockback;
            SourceTeam = sourceTeam;
        }
    }
}
