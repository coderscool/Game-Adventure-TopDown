using System;
using UnityEngine;
using UnityEngine.Serialization;
using Game.Features.Character.Model;

namespace Game.Features.Character.Systems
{
    public class Player : MonoBehaviour, IPlayerState
    {
        public static Player Instance { get; private set; }

        /// <summary>Raised by SavePlayer(); whoever owns persistence (GameManager) handles it.</summary>
        public static event Action SaveRequested;

        /// <summary>Raised after the player is moved instantly (teleport / spawn), so the camera can cut.</summary>
        public static event Action Warped;

        [FormerlySerializedAs("gold")] [SerializeField] private int _gold = 100;
        [FormerlySerializedAs("exp")] [SerializeField] private int _exp;
        [FormerlySerializedAs("exps")] [SerializeField] private int _exps;

        [FormerlySerializedAs("maxHp")] [SerializeField] private float _maxHp = 100f;
        [FormerlySerializedAs("hp")] [SerializeField] private float _hp = 100f;
        [FormerlySerializedAs("maxEnergy")] [SerializeField] private float _maxEnergy = 100f;
        [FormerlySerializedAs("energy")] [SerializeField] private float _energy = 100f;
        [FormerlySerializedAs("maxSpirit")] [SerializeField] private float _maxSpirit = 100f;
        [FormerlySerializedAs("spirit")] [SerializeField] private float _spirit = 100f;

        [FormerlySerializedAs("currentMapId")]
        [SerializeField] private string _currentMapId = "NoviceMap";

        /// <summary>Raised whenever gold, exp or any stat changes (HUD listens to this).</summary>
        public event Action StatsChanged;

        /// <summary>Raised once when Hp reaches 0.</summary>
        public event Action Died;

        public int Gold { get => _gold; set { _gold = Mathf.Max(0, value); StatsChanged?.Invoke(); } }
        public int Exp { get => _exp; set { _exp = value; StatsChanged?.Invoke(); } }
        public int Exps { get => _exps; set { _exps = value; StatsChanged?.Invoke(); } }

        public float MaxHp
        {
            get => _maxHp;
            set { _maxHp = Mathf.Max(1f, value); _hp = Mathf.Min(_hp, _maxHp); StatsChanged?.Invoke(); }
        }

        public float Hp
        {
            get => _hp;
            set
            {
                bool wasAlive = _hp > 0f;
                _hp = Mathf.Clamp(value, 0f, _maxHp);
                StatsChanged?.Invoke();
                if (wasAlive && _hp <= 0f) Died?.Invoke();
            }
        }

        public float MaxEnergy
        {
            get => _maxEnergy;
            set { _maxEnergy = Mathf.Max(1f, value); _energy = Mathf.Min(_energy, _maxEnergy); StatsChanged?.Invoke(); }
        }

        public float Energy
        {
            get => _energy;
            set { _energy = Mathf.Clamp(value, 0f, _maxEnergy); StatsChanged?.Invoke(); }
        }

        public float MaxSpirit
        {
            get => _maxSpirit;
            set { _maxSpirit = Mathf.Max(1f, value); _spirit = Mathf.Min(_spirit, _maxSpirit); StatsChanged?.Invoke(); }
        }

        public float Spirit
        {
            get => _spirit;
            set { _spirit = Mathf.Clamp(value, 0f, _maxSpirit); StatsChanged?.Invoke(); }
        }

        public void TakeDamage(float amount) { if (amount > 0f) Hp -= amount; }
        public void Heal(float amount) { if (amount > 0f) Hp += amount; }

        /// <summary>Returns false (and spends nothing) when there is not enough energy.</summary>
        public bool TrySpendEnergy(float amount)
        {
            if (amount < 0f || _energy < amount) return false;
            Energy -= amount;
            return true;
        }

        public string CurrentMapId { get => _currentMapId; set => _currentMapId = value; }

        /// <summary>Runtime-only state (restored from save), not serialized in the scene.</summary>
        public bool IsOnBoat { get; set; }

        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>Moves the player instantly and notifies listeners (e.g. the camera).</summary>
        public void WarpTo(Vector3 position)
        {
            transform.position = position;
            Physics2D.SyncTransforms();
            Warped?.Invoke();
        }

        // Bound to a UnityEvent in the GameStart scene - keep the name.
        public void SavePlayer()
        {
            if (SaveRequested == null)
            {
                Debug.LogWarning("Save did not complete: no save handler.");
                return;
            }

            SaveRequested.Invoke();
        }
    }
}
