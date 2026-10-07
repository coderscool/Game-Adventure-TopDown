using UnityEngine;
using UnityEngine.Serialization;

public class Boat : MonoBehaviour
{
    private const KeyCode InteractKey = KeyCode.E;
    private const float DismountDistance = 2f;

    [FormerlySerializedAs("seatPoint")]
    [SerializeField] private Transform _seatPoint;

    private bool _isPlayerInRange;
    private Transform _player;

    private void Update()
    {
        if (!_isPlayerInRange || !Input.GetKeyDown(InteractKey))
            return;

        if (Player.Instance.IsOnBoat)
            Dismount();
        else
            Mount();
    }

    private void Mount()
    {
        Player.Instance.IsOnBoat = true;

        _player.SetParent(transform);
        _player.position = _seatPoint.position;
    }

    private void Dismount()
    {
        Player.Instance.IsOnBoat = false;

        _player.SetParent(null);
        _player.position = transform.position + Vector3.left * DismountDistance;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(GameTags.Player))
            return;

        _isPlayerInRange = true;
        _player = other.transform;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(GameTags.Player))
            _isPlayerInRange = false;
    }
}
