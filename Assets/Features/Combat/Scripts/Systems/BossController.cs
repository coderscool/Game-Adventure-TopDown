using System;
using System.Collections;
using UnityEngine;
using Game.Features.Combat.Model;

namespace Game.Features.Combat.Systems
{
    /// <summary>
    /// A boss: slams the ground around itself, and below half health becomes faster and also charges.
    /// Shows its health bar while fighting. Stays defeated once beaten (add BossPersistence).
    /// </summary>
    public class BossController : EnemyController
    {
        private static readonly Color ChargeTint = new Color(1f, 0.25f, 0.25f, 1f);

        /// <summary>A boss started the fight. The health bar listens to this.</summary>
        public static event Action<BossController> FightStarted;

        /// <summary>The fight ended (boss died or the player got away).</summary>
        public static event Action<BossController> FightEnded;

        [Header("Laser")]
        [Tooltip("The boss starts a laser when the player is this close.")]
        [SerializeField] private float _laserTriggerRange = 7f;
        [Tooltip("How far the beam reaches.")]
        [SerializeField] private float _laserLength = 9f;
        [Tooltip("How wide the damage area is.")]
        [SerializeField] private float _laserWidth = 0.8f;
        [SerializeField] private float _laserSeconds = 0.8f;
        [Tooltip("How often the beam hurts while it is on.")]
        [SerializeField] private float _laserTickSeconds = 0.25f;
        [SerializeField] private float _laserDamageMultiplier = 1f;
        [Tooltip("Extra pause after a laser before the next attack.")]
        [SerializeField] private float _laserCooldownSeconds = 1.2f;

        [Header("Laser art (optional, set by Build Golem Boss)")]
        [SerializeField] private Sprite[] _laserChargeFrames;
        [SerializeField] private Sprite[] _laserBeamFrames;
        [Tooltip("Stretches the beam picture. 1 = original thickness.")]
        [SerializeField] private float _laserArtThickness = 1.8f;

        [Header("Phase 2 (below this share of health)")]
        [Range(0.1f, 0.9f)]
        [SerializeField] private float _phaseTwoThreshold = 0.5f;
        [SerializeField] private float _phaseTwoSpeedMultiplier = 1.4f;
        [SerializeField] private float _chargeSpeed = 9f;
        [SerializeField] private float _chargeSeconds = 0.55f;
        [SerializeField] private float _chargeWindupSeconds = 0.6f;
        [SerializeField] private float _chargeDamageMultiplier = 1.3f;
        [Range(0f, 1f)]
        [SerializeField] private float _chargeChance = 0.5f;

        [Header("Animation (optional)")]
        [Tooltip("Drives the boss sprites. Leave empty for no animation.")]
        [SerializeField] private Animator _animator;
        [Tooltip("How long the death animation plays before the boss disappears.")]
        [SerializeField] private float _deathSeconds = 1.6f;

        private static readonly int LaserHash = Animator.StringToHash("Laser");
        private static readonly int GlowHash = Animator.StringToHash("Glow");
        private static readonly int ImmuneHash = Animator.StringToHash("Immune");
        private static readonly int DeathHash = Animator.StringToHash("Death");

        private LaserBeamFx _laserFx;
        private bool _phaseTwo;
        private bool _fightActive;

        public Health BossHealth => Health;

        protected override float AttackTriggerRange => _laserTriggerRange;

        protected override void Awake()
        {
            base.Awake();

            if (_animator == null)
                _animator = GetComponent<Animator>();
        }

        private void PlayAnimation(int triggerHash)
        {
            if (_animator != null && _animator.runtimeAnimatorController != null)
                _animator.SetTrigger(triggerHash);
        }

        protected override void Engage()
        {
            base.Engage();
            SetFight(true);
        }

        protected override void Disengage()
        {
            base.Disengage();
            SetFight(false);
        }

        protected override void OnDamaged(HitInfo hit)
        {
            if (!_phaseTwo && Health.Current <= Health.Max * _phaseTwoThreshold)
            {
                _phaseTwo = true;
                SpeedMultiplier = _phaseTwoSpeedMultiplier;
                Flasher.Flash(ChargeTint, 0.4f);
                PlayAnimation(ImmuneHash);
            }
        }

        protected override void OnDefeated()
        {
            SetFight(false);
            PlayAnimation(DeathHash);
        }

        protected override IEnumerator AttackRoutine(Vector2 direction)
        {
            if (_phaseTwo && UnityEngine.Random.value < _chargeChance)
                yield return ChargeRoutine(direction);
            else
                yield return LaserRoutine();
        }

        // The picture is 300 px wide at 100 px per unit and the beam starts about 50 px from its left edge.
        private const float ArtBeamLength = 2.5f;
        private const float ChargeArtScale = 1.5f;
        private const float BeamFramesPerSecond = 12f;

        private IEnumerator LaserRoutine()
        {
            // The aim is locked when the warning starts, so the player can step out of the line.
            Vector2 aim = DirectionToTarget();
            float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;

            if (_laserFx == null)
                _laserFx = LaserBeamFx.Create();

            PlayAnimation(LaserHash);
            Flasher.SetHold(Warning);

            bool hasArt = _laserChargeFrames != null && _laserChargeFrames.Length > 0
                && _laserBeamFrames != null && _laserBeamFrames.Length > 0;

            float windup = Data.WindupSeconds;
            float time = 0f;

            while (time < windup && !IsDead)
            {
                _laserFx.ShowBar(Position, angle, _laserLength, 0.12f, true);

                if (hasArt)
                {
                    int frame = Mathf.Min((int)(time / windup * _laserChargeFrames.Length), _laserChargeFrames.Length - 1);
                    _laserFx.ShowSprite(_laserChargeFrames[frame], Position, angle, Vector2.one * ChargeArtScale);
                }

                time += Time.deltaTime;
                yield return null;
            }

            Flasher.ClearHold();
            _laserFx.HideAll();

            if (IsDead)
                yield break;

            time = 0f;
            float nextDamageTime = 0f;
            Vector2 boxSize = new Vector2(_laserLength, _laserWidth);

            while (time < _laserSeconds && !IsDead)
            {
                Vector2 origin = Position;

                if (hasArt)
                {
                    // First frame is the muzzle flash; the rest loop while the beam is on.
                    int index = time < 1f / BeamFramesPerSecond
                        ? 0
                        : 1 + (int)(time * BeamFramesPerSecond) % Mathf.Max(1, _laserBeamFrames.Length - 1);
                    index = Mathf.Min(index, _laserBeamFrames.Length - 1);

                    _laserFx.ShowSprite(_laserBeamFrames[index], origin, angle,
                        new Vector2(_laserLength / ArtBeamLength, _laserArtThickness));
                }
                else
                {
                    _laserFx.ShowBar(origin, angle, _laserLength, _laserWidth * 0.6f, false);
                }

                if (time >= nextDamageTime)
                {
                    nextDamageTime = time + _laserTickSeconds;
                    DamageInBox(origin + aim * (_laserLength * 0.5f), boxSize, angle, aim, Data.AttackDamage * _laserDamageMultiplier);
                }

                time += Time.deltaTime;
                yield return null;
            }

            _laserFx.HideAll();

            yield return new WaitForSeconds(Data.RecoverSeconds + _laserCooldownSeconds);
        }

        private IEnumerator ChargeRoutine(Vector2 direction)
        {
            PlayAnimation(GlowHash);
            Flasher.SetHold(ChargeTint);
            yield return new WaitForSeconds(_chargeWindupSeconds);
            Flasher.ClearHold();

            if (IsDead)
                yield break;

            float time = 0f;
            bool hasHit = false;

            while (time < _chargeSeconds && !IsDead)
            {
                MoveVelocity = direction * _chargeSpeed;

                if (!hasHit)
                {
                    Vector2 center = Position + direction * 0.6f;
                    hasHit = DamageInCircle(center, Data.AttackRadius, direction, Data.AttackDamage * _chargeDamageMultiplier);
                }

                time += Time.deltaTime;
                yield return null;
            }

            MoveVelocity = Vector2.zero;

            yield return new WaitForSeconds(Data.RecoverSeconds * 1.5f);
        }

        private Vector2 DirectionToTarget()
        {
            if (TargetTransform == null)
                return Vector2.down;

            Vector2 toTarget = (Vector2)TargetTransform.position - Position;
            return toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : Vector2.down;
        }

        private void SetFight(bool active)
        {
            if (_fightActive == active)
                return;

            _fightActive = active;

            if (active)
                FightStarted?.Invoke(this);
            else
                FightEnded?.Invoke(this);
        }

        private void OnDestroy()
        {
            if (_laserFx != null)
                Destroy(_laserFx.gameObject);
        }

        protected override void OnDisable()
        {
            if (_laserFx != null)
                _laserFx.HideAll();

            SetFight(false);
            base.OnDisable();
        }

        protected override IEnumerator DeathRoutine()
        {
            // Bosses are switched off (not destroyed) so BossPersistence can remember them.
            yield return new WaitForSeconds(_deathSeconds);
            gameObject.SetActive(false);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, _laserTriggerRange);
        }
    }
}
