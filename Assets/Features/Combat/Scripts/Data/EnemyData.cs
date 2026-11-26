using System.Collections.Generic;
using UnityEngine;

namespace Game.Features.Combat.Data
{
    /// <summary>All numbers of one kind of enemy (or boss). Create assets from Create > Combat > Enemy.</summary>
    [CreateAssetMenu(menuName = "Combat/Enemy", fileName = "Enemy_New")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Used by quests: 'kill 5 of this enemy id'. Example: slime")]
        [SerializeField] private string _enemyId = "slime";
        [SerializeField] private string _displayName = "Slime";

        [Header("Stats")]
        [Min(1f)]
        [SerializeField] private float _maxHealth = 30f;
        [SerializeField] private float _moveSpeed = 2f;

        [Header("Senses")]
        [Tooltip("The enemy starts chasing when the player is this close.")]
        [SerializeField] private float _detectRange = 5f;
        [Tooltip("The enemy gives up when the player is farther than this.")]
        [SerializeField] private float _loseRange = 9f;

        [Header("Attack")]
        [SerializeField] private float _attackDamage = 8f;
        [Tooltip("Starts an attack when the player is this close.")]
        [SerializeField] private float _attackRange = 1.1f;
        [Tooltip("Radius of the hit area, placed in front of the enemy.")]
        [SerializeField] private float _attackRadius = 0.9f;
        [Tooltip("Warning time before the hit lands (the enemy glows).")]
        [SerializeField] private float _windupSeconds = 0.5f;
        [Tooltip("Pause after the hit before the enemy moves again.")]
        [SerializeField] private float _recoverSeconds = 0.6f;
        [SerializeField] private float _attackKnockback = 6f;

        [Header("Rewards")]
        [SerializeField] private int _goldMin = 3;
        [SerializeField] private int _goldMax = 8;
        [SerializeField] private int _exp = 5;
        [SerializeField] private List<EnemyDrop> _drops = new List<EnemyDrop>();

        public string EnemyId => _enemyId;
        public string DisplayName => _displayName;
        public float MaxHealth => _maxHealth;
        public float MoveSpeed => _moveSpeed;
        public float DetectRange => _detectRange;
        public float LoseRange => Mathf.Max(_loseRange, _detectRange);
        public float AttackDamage => _attackDamage;
        public float AttackRange => _attackRange;
        public float AttackRadius => _attackRadius;
        public float WindupSeconds => _windupSeconds;
        public float RecoverSeconds => _recoverSeconds;
        public float AttackKnockback => _attackKnockback;
        public int GoldMin => _goldMin;
        public int GoldMax => Mathf.Max(_goldMin, _goldMax);
        public int Exp => _exp;
        public IReadOnlyList<EnemyDrop> Drops => _drops;
    }
}
