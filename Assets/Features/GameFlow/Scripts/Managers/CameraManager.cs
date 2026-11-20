using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Game.Features.Character.Systems;
using Game.Features.World;

namespace Game.Features.GameFlow.Managers
{
    /// <summary>
    /// Persistent camera rig. On every scene load it follows the player, applies the map's
    /// CameraBounds, and cuts (no damping) so the camera never slides across maps.
    /// Must sit on the root GameObject of the rig (DontDestroyOnLoad only works on roots).
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance { get; private set; }

        [FormerlySerializedAs("vcam")]
        [SerializeField] private CinemachineVirtualCamera _vcam;

        [SerializeField] private CinemachineConfiner2D _confiner;

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

        private void OnEnable()
        {
            if (Instance != this)
                return;

            SceneManager.sceneLoaded += OnSceneLoaded;
            Player.Warped += Cut;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Player.Warped -= Cut;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            FollowPlayer();
            ApplyBounds();
            Cut();
        }

        private void FollowPlayer()
        {
            Player player = Player.Instance;

            if (player == null || _vcam == null)
            {
                Debug.LogWarning("CameraManager: missing Player or virtual camera.");
                return;
            }

            _vcam.Follow = player.transform;
            _vcam.LookAt = player.transform;
        }

        private void ApplyBounds()
        {
            if (_vcam == null)
                return;

            CameraBounds bounds = FindFirstObjectByType<CameraBounds>();

            if (_confiner != null)
            {
                _confiner.m_BoundingShape2D = bounds != null ? bounds.Shape : null;
                _confiner.InvalidateCache();
            }

            if (bounds != null && bounds.OrthoSize > 0f)
                _vcam.m_Lens.OrthographicSize = bounds.OrthoSize;
        }

        /// <summary>Jump to the target without damping (no sliding across the map).</summary>
        private void Cut()
        {
            if (_vcam != null)
                _vcam.PreviousStateIsValid = false;
        }
    }
}
