namespace Game.Features.Combat.Model
{
    /// <summary>Anything that can be hit: the player, enemies, bosses.</summary>
    public interface IDamageable
    {
        DamageTeam Team { get; }

        bool IsAlive { get; }

        void TakeHit(HitInfo hit);
    }
}
