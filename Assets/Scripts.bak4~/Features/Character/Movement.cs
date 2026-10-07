using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Features.Character
{
    public class Movement : MonoBehaviour
    {
        [FormerlySerializedAs("moveSpeed")]
        [SerializeField] private float _moveSpeed = 5f;

        [FormerlySerializedAs("rb")]
        [SerializeField] private Rigidbody2D _rigidbody;

        private Vector2 _input;

        private void Update()
        {
            _input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        }

        private void FixedUpdate()
        {
            _rigidbody.MovePosition(_rigidbody.position + _input * _moveSpeed * Time.fixedDeltaTime);
        }
    }
}
