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

        // Last direction the player moved in; kept while standing still so Idle faces the right way.
        private Vector2 _facing = Vector2.down;

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
            _input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

            bool isMoving = _input != Vector2.zero;
            if (isMoving)
                _facing = _input;

            UpdateAnimator(isMoving);
        }

        private void FixedUpdate()
        {
            _rigidbody.MovePosition(_rigidbody.position + _input * _moveSpeed * Time.fixedDeltaTime);
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
