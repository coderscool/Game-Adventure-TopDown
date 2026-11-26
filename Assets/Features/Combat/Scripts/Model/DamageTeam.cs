namespace Game.Features.Combat.Model
{
    /// <summary>Who an attacker or a target belongs to. Attacks never hurt their own team.</summary>
    public enum DamageTeam
    {
        Player,
        Enemy
    }
}
