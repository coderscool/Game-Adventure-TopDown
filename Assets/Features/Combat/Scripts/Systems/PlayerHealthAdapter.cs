using System.Collections;
using UnityEngine;
using Game.Features.Character.Systems;
using Game.Features.Combat.Model;
using Game.Features.GameFlow.Managers;

namespace Game.Features.Combat.Systems
{
    /// <summary>
    /// Lets enemies hurt the player: after a hit the player is briefly protected and pushed back.
    /// When health runs out the player waits a moment and is sent back to the start map.
    /// </summary>
    public class PlayerHealthAdapter : MonoBehaviour, IDamageable
    {
        private static readonly Color HurtTint = new Color(1f, 0.4f, 0.4f, 1f);

        [SerializeField] private float _invulnerableSeconds = 0.8f;
        [SerializeField] private float _knockbackSeconds = 0.15f;
        [SerializeField] private float _respawnDelaySeconds = 1.5f;

        private Player _player;
        private Movement _movement;
        private PlayerCombat _combat;
        private SpriteFlasher _flasher;
        private float _invulnerableUntil;
        private bool _isDead;

        public DamageTeam Team => DamageTeam.Player;

        public bool IsAlive => !_isDead && _player != null && _player.Hp > 0f;

        private void Awake()
        {
            _player = GetComponent<Player>();
            _movement = GetComponent<Movement>();
            _combat = GetComponent<PlayerCombat>();
            _flasher = GetComponent<SpriteFlasher>();
        }

        private void OnEnable()
        {
            if (_player != null)
                _player.Died += OnDied;
        }

        private void OnDisable()
        {
            if (_player != null)
                _player.Died -= OnDied;
        }

        public void TakeHit(HitInfo hit)
        {
            if (!IsAlive || Time.time < _invulnerableUntil)
                return;

            _invulnerableUntil = Time.time + _invulnerableSeconds;
            _player.TakeDamage(hit.Damage);

            if (_movement != null)
                _movement.AddKnockback(hit.Direction * hit.Knockback, _knockbackSeconds);

            if (_flasher != null)
                _flasher.Flash(HurtTint, 0.2f);
        }

        private void OnDied()
        {
            if (_isDead)
                return;

            _isDead = true;

            if (_combat != null)
                _combat.enabled = false;

            if (_movement != null)
                _movement.LockInput(_respawnDelaySeconds + 0.5f);

            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(_respawnDelaySeconds);

            if (GameManager.Instance != null)
                GameManager.Instance.RespawnPlayer();
            else
                _player.Hp = _player.MaxHp;

            _invulnerableUntil = Time.time + 2f;
            _isDead = false;

            if (_combat != null)
                _combat.enabled = true;
        }
    }
}
