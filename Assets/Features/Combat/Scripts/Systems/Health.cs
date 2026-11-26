using System;
using UnityEngine;
using Game.Features.Combat.Model;

namespace Game.Features.Combat.Systems
{
    /// <summary>Hit points of an enemy or boss. Raises events so other scripts can react.</summary>
    public class Health : MonoBehaviour
    {
        [Min(1f)]
        [SerializeField] private float _maxHealth = 30f;

        private float _current;
        private bool _initialized;

        public float Max => _maxHealth;
        public float Current
        {
            get
            {
                EnsureInitialized();
                return _current;
            }
        }

        public bool IsAlive => Current > 0f;

        /// <summary>Raised on every hit that lands.</summary>
        public event Action<HitInfo> Damaged;

        /// <summary>Raised whenever the current or maximum value changes.</summary>
        public event Action Changed;

        /// <summary>Raised once, when health reaches 0.</summary>
        public event Action Died;

        /// <summary>Sets the maximum (e.g. from EnemyData) and fills health.</summary>
        public void Setup(float maxHealth)
        {
            _maxHealth = Mathf.Max(1f, maxHealth);
            _current = _maxHealth;
            _initialized = true;
            Changed?.Invoke();
        }

        public void RestoreFull()
        {
            _current = _maxHealth;
            _initialized = true;
            Changed?.Invoke();
        }

        public void Hit(HitInfo hit)
        {
            EnsureInitialized();

            if (_current <= 0f)
                return;

            _current = Mathf.Max(0f, _current - hit.Damage);

            Damaged?.Invoke(hit);
            Changed?.Invoke();

            if (_current <= 0f)
                Died?.Invoke();
        }

        private void EnsureInitialized()
        {
            if (_initialized)
                return;

            _current = _maxHealth;
            _initialized = true;
        }
    }
}
