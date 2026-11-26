using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Features.Character.Systems
{
    public class Movement : MonoBehaviour
    {
        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

        [FormerlySerializedAs("moveSpeed")]
        [SerializeField] private float _moveSpeed = 5f;

        [FormerlySerializedAs("rb")]
        [SerializeField] private Rigidbody2D _rigidbody;

        [SerializeField] private Animator _animator;

        private Vector2 _input;

        private float _inputLockedUntil;
        private Vector2 _knockbackVelocity;
        private float _knockbackUntil;

        // Last direction the player moved in; kept while standing still so Idle faces the right way.
        private Vector2 _facing = Vector2.down;

        /// <summary>The way the character is looking (last moved direction). Attacks aim along it.</summary>
        public Vector2 Facing => _facing;

        /// <summary>Stops walking input for a moment, e.g. while attacking or after dying.</summary>
        public void LockInput(float seconds)
        {
            _inputLockedUntil = Mathf.Max(_inputLockedUntil, Time.time + seconds);
        }

        /// <summary>Pushes the character at the given velocity for a short time.</summary>
        public void AddKnockback(Vector2 velocity, float seconds)
        {
            _knockbackVelocity = velocity;
            _knockbackUntil = Time.time + seconds;
        }

        private void Awake()
        {
            // A sleeping body can ignore MovePosition after standing still, which feels like a stutter.
            _rigidbody.sleepMode = RigidbodySleepMode2D.NeverSleep;

            if (_animator == null)
                _animator = GetComponent<Animator>();

            UpdateAnimator(false);
        }

        private void Update()
        {
            _input = Time.time < _inputLockedUntil
                ? Vector2.zero
                : new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

            bool isMoving = _input != Vector2.zero;
            if (isMoving)
                _facing = _input;

            UpdateAnimator(isMoving);
        }

        private void FixedUpdate()
        {
            Vector2 velocity = _input * _moveSpeed;

            if (Time.time < _knockbackUntil)
                velocity += _knockbackVelocity;

            _rigidbody.MovePosition(_rigidbody.position + velocity * Time.fixedDeltaTime);
        }

        private void UpdateAnimator(bool isMoving)
        {
            if (_animator == null)
                return;

            _animator.SetFloat(MoveXHash, _facing.x);
            _animator.SetFloat(MoveYHash, _facing.y);
            _animator.SetBool(IsMovingHash, isMoving);
        }
    }
}
