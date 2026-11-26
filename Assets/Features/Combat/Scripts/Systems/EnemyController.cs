using System;
using System.Collections;
using UnityEngine;
using Game.Features.Character.Systems;
using Game.Features.Combat.Data;
using Game.Features.Combat.Model;
using Game.Features.Inventory;
using Game.Features.Quest.Systems;

namespace Game.Features.Combat.Systems
{
    /// <summary>
    /// A simple melee enemy: wanders, chases the player when close, warns (glows), then hits.
    /// Works without animations. Bosses derive from this class.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour, IDamageable
    {
        private static readonly Color HitTint = new Color(1f, 0.3f, 0.3f, 0.6f);
        private static readonly Color WarningTint = new Color(1f, 0.55f, 0.2f, 1f);

        private const float WanderRadius = 3f;
        private const float DeathSeconds = 0.35f;

        /// <summary>Raised by every enemy when it dies.</summary>
        public static event Action<EnemyController> AnyDied;

        [SerializeField] private EnemyData _data;

        private Rigidbody2D _rigidbody;
        private Health _health;
        private SpriteFlasher _flasher;
        private IDamageable _target;
        private Transform _targetTransform;
        private Vector2 _home;
        private Vector2 _moveVelocity;
        private Vector2 _knockbackVelocity;
        private float _knockbackUntil;
        private float _nextWanderTime;
        private Vector2 _wanderDirection;
        private bool _isAttacking;
        private bool _isDead;

        protected Color Warning => WarningTint;
        protected EnemyData Data => _data;
        protected Health Health => _health;
        protected SpriteFlasher Flasher => _flasher;
        protected Transform TargetTransform => _targetTransform;
        protected Vector2 Position => _rigidbody.position;

        /// <summary>Set by subclasses to take over movement (e.g. a charge). Reset to zero when the attack ends.</summary>
        protected Vector2 MoveVelocity
        {
            get => _moveVelocity;
            set => _moveVelocity = value;
        }

        /// <summary>Multiplies walking speed. Bosses raise it in later phases.</summary>
        protected float SpeedMultiplier { get; set; } = 1f;

        /// <summary>How close the player must be for an attack to start. Ranged bosses raise it.</summary>
        protected virtual float AttackTriggerRange => _data.AttackRange;

        public EnemyData EnemyInfo => _data;

        public bool IsDead => _isDead;

        public DamageTeam Team => DamageTeam.Enemy;

        public bool IsAlive => !_isDead;

        /// <summary>Raised once when this enemy dies.</summary>
        public event Action<EnemyController> Defeated;

        /// <summary>Called by a spawner right after the enemy is created.</summary>
        public void Initialize(EnemyData data)
        {
            _data = data;
            ApplyData();
        }

        protected virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.gravityScale = 0f;
            _rigidbody.freezeRotation = true;
            _rigidbody.sleepMode = RigidbodySleepMode2D.NeverSleep;
            _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;

            _health = GetComponent<Health>();
            if (_health == null)
                _health = gameObject.AddComponent<Health>();

            _flasher = GetComponent<SpriteFlasher>();
            if (_flasher == null)
                _flasher = gameObject.AddComponent<SpriteFlasher>();

            _home = _rigidbody.position;
        }

        protected virtual void Start()
        {
            ApplyData();
        }

        protected virtual void OnEnable()
        {
            _health = _health != null ? _health : GetComponent<Health>();
            if (_health != null)
                _health.Died += OnHealthDied;
        }

        protected virtual void OnDisable()
        {
            if (_health != null)
                _health.Died -= OnHealthDied;

            _isAttacking = false;
        }

        private void ApplyData()
        {
            if (_data == null || _health == null)
                return;

            _health.Setup(_data.MaxHealth);
            _home = _rigidbody != null ? _rigidbody.position : (Vector2)transform.position;
        }

        private void Update()
        {
            if (_isDead || _data == null)
                return;

            if (!FindTarget())
            {
                Wander();
                return;
            }

            if (_isAttacking)
                return;

            float distance = Vector2.Distance(_rigidbody.position, _targetTransform.position);

            if (distance > _data.LoseRange || (!IsEngaged && distance > _data.DetectRange))
            {
                if (IsEngaged)
                    Disengage();

                Wander();
                return;
            }

            if (!IsEngaged)
                Engage();

            Vector2 toTarget = ((Vector2)_targetTransform.position - _rigidbody.position);
            Vector2 direction = toTarget.normalized;

            if (distance <= AttackTriggerRange)
            {
                _moveVelocity = Vector2.zero;
                StartCoroutine(AttackWrapper(direction));
            }
            else
            {
                _moveVelocity = direction * (_data.MoveSpeed * SpeedMultiplier);
            }
        }

        private void FixedUpdate()
        {
            if (_isDead)
                return;

            Vector2 velocity = _moveVelocity;

            if (Time.time < _knockbackUntil)
                velocity += _knockbackVelocity;

            _rigidbody.MovePosition(_rigidbody.position + velocity * Time.fixedDeltaTime);
        }

        /// <summary>True while the enemy is chasing the player.</summary>
        protected bool IsEngaged { get; private set; }

        protected virtual void Engage()
        {
            IsEngaged = true;
        }

        protected virtual void Disengage()
        {
            IsEngaged = false;
            _moveVelocity = Vector2.zero;
        }

        private bool FindTarget()
        {
            if (_targetTransform == null)
            {
                Player player = Player.Instance;
                if (player == null)
                    return false;

                _targetTransform = player.transform;
                _target = player.GetComponent<IDamageable>();
            }

            return _target != null && _target.IsAlive;
        }

        private void Wander()
        {
            if (Time.time >= _nextWanderTime)
            {
                _nextWanderTime = Time.time + UnityEngine.Random.Range(1.5f, 3.5f);

                Vector2 toHome = _home - _rigidbody.position;
                if (toHome.magnitude > WanderRadius)
                    _wanderDirection = toHome.normalized;
                else if (UnityEngine.Random.value < 0.4f)
                    _wanderDirection = Vector2.zero;
                else
                    _wanderDirection = UnityEngine.Random.insideUnitCircle.normalized;
            }

            _moveVelocity = _wanderDirection * (_data.MoveSpeed * 0.4f);
        }

        private IEnumerator AttackWrapper(Vector2 direction)
        {
            _isAttacking = true;

            yield return AttackRoutine(direction);

            _flasher.ClearHold();
            _moveVelocity = Vector2.zero;
            _isAttacking = false;
        }

        /// <summary>One whole attack: warn, hit, recover. Bosses override this with their own moves.</summary>
        protected virtual IEnumerator AttackRoutine(Vector2 direction)
        {
            _flasher.SetHold(WarningTint);
            yield return new WaitForSeconds(_data.WindupSeconds);
            _flasher.ClearHold();

            if (_isDead)
                yield break;

            HitInFront(direction, _data.AttackRange * 0.6f, _data.AttackRadius, _data.AttackDamage);

            yield return new WaitForSeconds(_data.RecoverSeconds);
        }

        /// <summary>Damages the player if they are inside a circle placed in front of this enemy.</summary>
        protected void HitInFront(Vector2 direction, float forward, float radius, float damage)
        {
            Vector2 center = _rigidbody.position + direction * forward;
            DamageInCircle(center, radius, direction, damage);
        }

        /// <summary>Hurts the first player inside the circle. Returns true if someone was hit.</summary>
        protected bool DamageInCircle(Vector2 center, float radius, Vector2 direction, float damage)
        {
            foreach (Collider2D collider in Physics2D.OverlapCircleAll(center, radius))
            {
                IDamageable target = collider.GetComponentInParent<IDamageable>();

                if (target == null || target.Team != DamageTeam.Player || !target.IsAlive)
                    continue;

                target.TakeHit(new HitInfo(damage, direction, _data.AttackKnockback, DamageTeam.Enemy));
                return true;
            }

            return false;
        }

        /// <summary>Hurts the first player inside a rotated box. Returns true if someone was hit.</summary>
        protected bool DamageInBox(Vector2 center, Vector2 size, float angle, Vector2 direction, float damage)
        {
            foreach (Collider2D collider in Physics2D.OverlapBoxAll(center, size, angle))
            {
                IDamageable target = collider.GetComponentInParent<IDamageable>();

                if (target == null || target.Team != DamageTeam.Player || !target.IsAlive)
                    continue;

                target.TakeHit(new HitInfo(damage, direction, _data.AttackKnockback, DamageTeam.Enemy));
                return true;
            }

            return false;
        }

        public void TakeHit(HitInfo hit)
        {
            if (_isDead || hit.SourceTeam == DamageTeam.Enemy)
                return;

            _health.Hit(hit);
            _flasher.Flash(HitTint, 0.12f);

            _knockbackVelocity = hit.Direction * hit.Knockback;
            _knockbackUntil = Time.time + 0.15f;

            // Being hit also wakes the enemy up, even from outside its detect range.
            if (!_isDead && !IsEngaged)
                Engage();

            OnDamaged(hit);
        }

        protected virtual void OnDamaged(HitInfo hit)
        {
        }

        private void OnHealthDied()
        {
            if (_isDead)
                return;

            _isDead = true;
            _moveVelocity = Vector2.zero;
            _flasher.ClearHold();
            StopAllCoroutines();

            foreach (Collider2D collider in GetComponentsInChildren<Collider2D>())
                collider.enabled = false;

            GiveRewards();

            if (QuestManager.Instance != null && _data != null)
                QuestManager.Instance.ReportEnemyKilled(_data.EnemyId);

            Defeated?.Invoke(this);
            AnyDied?.Invoke(this);

            OnDefeated();
            StartCoroutine(DeathRoutine());
        }

        /// <summary>Hook for subclasses (e.g. bosses mark themselves as defeated).</summary>
        protected virtual void OnDefeated()
        {
        }

        /// <summary>What happens to the body after death. Default: shrink away, then destroy.</summary>
        protected virtual IEnumerator DeathRoutine()
        {
            Vector3 startScale = transform.localScale;
            float time = 0f;

            while (time < DeathSeconds)
            {
                time += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, time / DeathSeconds);
                yield return null;
            }

            Destroy(gameObject);
        }

        private void GiveRewards()
        {
            if (_data == null)
                return;

            Player player = Player.Instance;
            if (player != null)
            {
                player.Gold += UnityEngine.Random.Range(_data.GoldMin, _data.GoldMax + 1);
                player.Exp += _data.Exp;
            }

            InventoryController inventory = InventoryController.Instance;
            if (inventory == null)
                return;

            foreach (EnemyDrop drop in _data.Drops)
            {
                if (drop.Item == null || UnityEngine.Random.value > drop.Chance)
                    continue;

                if (!inventory.AddItem(drop.Item, drop.Amount))
                    Debug.Log($"Inventory full: {drop.Item.ItemName} was not picked up.");
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_data == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _data.DetectRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _data.AttackRange);
        }
    }
}
