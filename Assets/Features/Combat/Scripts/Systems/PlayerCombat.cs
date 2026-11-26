using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Features.Character.Systems;
using Game.Features.Combat.Model;

namespace Game.Features.Combat.Systems
{
    /// <summary>
    /// The player's melee attack. Press Space to hit everything in a box in front of the player
    /// (the way the player is facing). Works without animations: a slash sprite flashes instead.
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        private const KeyCode AttackKey = KeyCode.Space;
        private const string SortingLayerName = "Characters";
        private const int FxSortingOrder = 30000;

        [SerializeField] private Movement _movement;

        [Header("Attack")]
        [SerializeField] private float _damage = 10f;
        [Tooltip("How far the hit box reaches in front of the player.")]
        [SerializeField] private float _range = 1.2f;
        [Tooltip("How wide the hit box is.")]
        [SerializeField] private float _width = 1.4f;
        [SerializeField] private float _knockback = 5f;
        [SerializeField] private float _cooldownSeconds = 0.4f;
        [Tooltip("The player cannot walk for this long after attacking.")]
        [SerializeField] private float _lockSeconds = 0.2f;

        [Header("Placeholder slash")]
        [SerializeField] private float _slashSeconds = 0.12f;
        [SerializeField] private Color _slashColor = new Color(1f, 1f, 1f, 0.6f);

        private SpriteRenderer _slash;
        private Coroutine _slashRoutine;
        private float _nextAttackTime;

        private void Awake()
        {
            if (_movement == null)
                _movement = GetComponent<Movement>();

            CreateSlash();
        }

        private void Update()
        {
            if (Input.GetKeyDown(AttackKey) && Time.time >= _nextAttackTime)
                Attack();
        }

        private void OnDisable()
        {
            if (_slash != null)
                _slash.enabled = false;
        }

        private void Attack()
        {
            Vector2 direction = _movement != null ? _movement.Facing : Vector2.down;
            if (direction.sqrMagnitude < 0.01f)
                direction = Vector2.down;

            direction.Normalize();

            _nextAttackTime = Time.time + _cooldownSeconds;

            if (_movement != null)
                _movement.LockInput(_lockSeconds);

            Vector2 center = (Vector2)transform.position + direction * (_range * 0.5f);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            Collider2D[] colliders = Physics2D.OverlapBoxAll(center, new Vector2(_range, _width), angle);
            var alreadyHit = new HashSet<IDamageable>();

            foreach (Collider2D collider in colliders)
            {
                IDamageable target = collider.GetComponentInParent<IDamageable>();

                if (target == null || target.Team == DamageTeam.Player || !target.IsAlive || !alreadyHit.Add(target))
                    continue;

                target.TakeHit(new HitInfo(_damage, direction, _knockback, DamageTeam.Player));
            }

            ShowSlash(center, angle);
        }

        private void CreateSlash()
        {
            var slashObject = new GameObject("SlashFx");
            slashObject.transform.SetParent(transform, false);

            _slash = slashObject.AddComponent<SpriteRenderer>();
            _slash.sprite = PlaceholderSprites.Square;
            _slash.color = _slashColor;
            _slash.sortingLayerName = SortingLayerName;
            _slash.sortingOrder = FxSortingOrder;
            _slash.enabled = false;
        }

        private void ShowSlash(Vector2 center, float angle)
        {
            Transform fx = _slash.transform;
            fx.position = center;
            fx.rotation = Quaternion.Euler(0f, 0f, angle);
            fx.localScale = new Vector3(_range, _width * 0.5f, 1f);

            if (_slashRoutine != null)
                StopCoroutine(_slashRoutine);

            _slashRoutine = StartCoroutine(HideSlashAfterDelay());
        }

        private IEnumerator HideSlashAfterDelay()
        {
            _slash.enabled = true;
            yield return new WaitForSeconds(_slashSeconds);
            _slash.enabled = false;
            _slashRoutine = null;
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 direction = _movement != null && _movement.Facing.sqrMagnitude > 0.01f ? _movement.Facing.normalized : Vector2.down;
            Vector2 center = (Vector2)transform.position + direction * (_range * 0.5f);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            Gizmos.color = Color.yellow;
            Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0f, 0f, angle), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(_range, _width, 0f));
        }
    }
}
