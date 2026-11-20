using UnityEngine;

namespace Game.Features.World
{
    /// <summary>Marks the area the camera may show in this map. One per map scene.</summary>
    [RequireComponent(typeof(PolygonCollider2D))]
    public class CameraBounds : MonoBehaviour
    {
        [SerializeField] private PolygonCollider2D _shape;

        [Tooltip("Camera zoom for this map. 0 keeps the current zoom.")]
        [SerializeField] private float _orthoSize;

        public PolygonCollider2D Shape => _shape;
        public float OrthoSize => _orthoSize;

        private void Reset()
        {
            _shape = GetComponent<PolygonCollider2D>();
        }
    }
}
