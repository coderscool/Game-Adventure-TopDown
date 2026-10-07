using UnityEngine;
using UnityEngine.Serialization;

public class YSort : MonoBehaviour
{
    private const string SortingLayerName = "Characters";
    private const int SortingPrecision = 100;

    [FormerlySerializedAs("sortPoint")]
    [SerializeField] private Transform _sortPoint;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sortingLayerName = SortingLayerName;

        if (_sortPoint == null)
            _sortPoint = transform;
    }

    private void LateUpdate()
    {
        _spriteRenderer.sortingOrder = Mathf.RoundToInt(-_sortPoint.position.y * SortingPrecision);
    }
}
