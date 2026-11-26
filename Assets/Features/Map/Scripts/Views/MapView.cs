using UnityEngine;
using Game.Features.Character.Systems;
using Game.Features.GameFlow.Managers;
using Game.Features.Inventory;
using Game.Features.UI;

namespace Game.Features.Map.Views
{
    /// <summary>
    /// The world map window. Put it on the map panel; the panel itself is opened and closed by
    /// UIManager.ToggleMap (button or M key). Clicking a point travels there with the loading screen.
    /// Points with a Required Item are only usable while the player owns that item.
    /// </summary>
    public class MapView : MonoBehaviour
    {
        private MapPointView[] _points;

        private void Awake()
        {
            _points = GetComponentsInChildren<MapPointView>(true);

            foreach (MapPointView point in _points)
            {
                MapPointView captured = point;
                point.Button.onClick.AddListener(() => OnPointClicked(captured));
            }
        }

        // Each time the map is opened, re-check where the player is and what they own.
        private void OnEnable()
        {
            if (_points == null)
                return;

            Player player = Player.Instance;
            string currentMap = player != null ? player.CurrentMapId : null;

            foreach (MapPointView point in _points)
                point.Refresh(point.SceneName == currentMap, IsUnlocked(point));
        }

        private static bool IsUnlocked(MapPointView point)
        {
            if (point.RequiredItem == null)
                return true;

            InventoryController inventory = InventoryController.Instance;
            return inventory != null && inventory.HasItem(point.RequiredItem);
        }

        private void OnPointClicked(MapPointView point)
        {
            if (!IsUnlocked(point))
                return;

            if (GameManager.Instance == null)
            {
                Debug.LogWarning("MapView: no GameManager.");
                return;
            }

            if (UIManager.Instance != null)
                UIManager.Instance.CloseAll();

            GameManager.Instance.TravelTo(point.SceneName, point.SpawnId);
        }
    }
}
