using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Features.Character.Systems;
using Game.Features.Inventory;
using Game.Features.Save;
using Game.Features.Save.Interfaces;
using Game.Features.Save.Services;

namespace Game.Features.GameFlow.Managers
{
    /// <summary>
    /// Composition root: creates the services and wires feature events together.
    /// Features raise events; this class decides who handles them.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class GameManager : MonoBehaviour
    {
        private const float PostLoadDelaySeconds = 0.1f;

        public static GameManager Instance { get; private set; }

        private SaveGameService _saveGameService;
        private GameLoadService _gameLoadService;
        private SceneStateService _sceneStateService;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            ISaveStorage storage = new SaveSystem();
            _saveGameService = new SaveGameService(storage);
            _gameLoadService = new GameLoadService(storage);
            _sceneStateService = new SceneStateService(storage);
        }

        private void Start()
        {
            _gameLoadService?.TryRestoreFromSave(Player.Instance, InventoryController.Instance);
        }

        private void OnEnable()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            if (Instance != this)
                return;

            SaveableBehaviour.Registered += Register;
            SaveableBehaviour.Unregistered += Unregister;
            SaveableBehaviour.SceneSaveRequested += SaveSceneState;
            Player.SaveRequested += OnPlayerSaveRequested;
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            SaveableBehaviour.Registered -= Register;
            SaveableBehaviour.Unregistered -= Unregister;
            SaveableBehaviour.SceneSaveRequested -= SaveSceneState;
            Player.SaveRequested -= OnPlayerSaveRequested;
        }

        public bool SaveGame()
        {
            return _saveGameService != null
                && _saveGameService.TrySave(Player.Instance, InventoryController.Instance);
        }

        /// <summary>Loads the map stored in the player's save/state, with the loading screen.</summary>
        public void LoadScene()
        {
            Player player = Player.Instance;
            if (player == null)
            {
                Debug.LogWarning("LoadScene: Player.Instance is null.");
                return;
            }

            StartCoroutine(SceneTransitionService.LoadSceneWithTransition(
                player.CurrentMapId,
                RestoreSceneStateAfterLoad));
        }

        private void OnPlayerSaveRequested()
        {
            if (!SaveGame())
                Debug.LogWarning("Save did not complete.");
        }

        private void Register(ISaveable obj, string sceneName)
        {
            _sceneStateService?.Register(obj, sceneName);
        }

        private void Unregister(ISaveable obj, string sceneName)
        {
            _sceneStateService?.Unregister(obj, sceneName);
        }

        private void SaveSceneState(string sceneName)
        {
            _sceneStateService?.SaveSceneState(sceneName);
        }

        private IEnumerator RestoreSceneStateAfterLoad()
        {
            _sceneStateService.LoadSceneState(SceneManager.GetActiveScene().name);

            yield return new WaitForSeconds(PostLoadDelaySeconds);
        }

        private void OnSceneUnloaded(Scene scene)
        {
            _sceneStateService?.SaveSceneState(scene.name);
            _sceneStateService?.ClearRegistry(scene.name);
        }
    }
}
