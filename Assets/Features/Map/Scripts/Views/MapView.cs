using UnityEngine;
using Game.Features.Character.Systems;
using Game.Features.GameFlow.Managers;
using Game.Features.UI;

namespace Game.Features.Map.Views
{
    /// <summary>
    /// The world map window. Put it on the map panel; the panel itself is opened and closed by
    /// UIManager.ToggleMap (button or M key). Clicking a point travels there with the loading screen.
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

        // Each time the map is opened, mark where the player currently is.
        private void OnEnable()
        {
            Player player = Player.Instance;
            string currentMap = player != null ? player.CurrentMapId : null;

            if (_points == null)
                return;

            foreach (MapPointView point in _points)
                point.SetCurrent(point.SceneName == currentMap);
        }

        private void OnPointClicked(MapPointView point)
        {
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
