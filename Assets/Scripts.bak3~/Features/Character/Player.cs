using UnityEngine;
using UnityEngine.Serialization;
using Game.Features.GameFlow;

namespace Game.Features.Character
{
    public class Player : MonoBehaviour, IPlayerState
    {
        public static Player Instance { get; private set; }

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
        [SerializeField] private string _currentMapId = "SampleScene";

        public int Gold { get => _gold; set => _gold = value; }
        public int Exp { get => _exp; set => _exp = value; }
        public int Exps { get => _exps; set => _exps = value; }

        public float MaxHp { get => _maxHp; set => _maxHp = value; }
        public float Hp { get => _hp; set => _hp = value; }
        public float MaxEnergy { get => _maxEnergy; set => _maxEnergy = value; }
        public float Energy { get => _energy; set => _energy = value; }
        public float MaxSpirit { get => _maxSpirit; set => _maxSpirit = value; }
        public float Spirit { get => _spirit; set => _spirit = value; }

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

        // Bound to a UnityEvent in the GameStart scene - keep the name.
        public void SavePlayer()
        {
            if (GameManager.Instance == null || !GameManager.Instance.SaveGame())
                Debug.LogWarning("Save did not complete.");
        }
    }
}
