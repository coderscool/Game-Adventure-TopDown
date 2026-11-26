using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Features.Character.Systems;
using Game.Features.Inventory;
using Game.Features.Quest.Systems;
using Game.Features.Save;
using Game.Features.Save.Interfaces;
using Game.Features.Save.Services;
using Game.Features.World;

namespace Game.Features.GameFlow.Managers
{
    /// <summary>
    /// Composition root: creates the services and wires feature events together.
    /// Features raise events; this class decides who handles them.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class GameManager : MonoBehaviour
    {
        [Header("New game")]
        [SerializeField] private int _newGameGold = 500;
        [SerializeField] private string _newGameMapId = "NoviceMap";
        [SerializeField] private string _newGameSpawnId = "PlayerStart";

        [Header("Boot")]
        [SerializeField] private string _bootSceneName = "GameStart";

        private const float PostLoadDelaySeconds = 0.1f;

        public static GameManager Instance { get; private set; }

        private SaveGameService _saveGameService;
        private GameLoadService _gameLoadService;
        private SceneStateService _sceneStateService;
        private bool _autoSaveEnabled = true;
        private bool _isTraveling;

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
            if (SceneManager.GetActiveScene().name != _bootSceneName)
            {
                StartInCurrentScene();
                return;
            }

            bool hasSave = _gameLoadService.TryRestoreFromSave(Player.Instance, InventoryController.Instance, QuestManager.Instance);

            if (!hasSave)
            {
                StartNewGame();
                return;
            }

            Player player = Player.Instance;
            if (player != null && !Application.CanStreamedLevelBeLoaded(player.CurrentMapId))
            {
                // e.g. the saved map was renamed/removed: do not get stuck, go to the start spawn.
                Debug.LogWarning($"Saved map '{player.CurrentMapId}' is not in Build Settings; using the start map.");
                EnterStartMap(player);
                return;
            }

            LoadScene();
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

        public void StartNewGame()
        {
            Player player = Player.Instance;
            if (player == null)
            {
                Debug.LogWarning("StartNewGame: Player.Instance is null.");
                return;
            }
            player.Gold = _newGameGold;
            EnterStartMap(player);
        }

        /// <summary>Loads the start map behind the loading screen and puts the player on the start spawn.</summary>
        private void EnterStartMap(Player player)
        {
            player.CurrentMapId = _newGameMapId;

            StartCoroutine(SceneTransitionService.LoadSceneWithTransition(
                _newGameMapId, PlaceNewPlayerAfterLoad));
        }

        private IEnumerator PlaceNewPlayerAfterLoad()
        {
            yield return RestoreSceneStateAfterLoad();
            TeleportManager.MovePlayerToSpawn(_newGameSpawnId);

            // One frame so the camera cut is applied while the loading screen is still showing.
            yield return null;

            SaveGame();
        }

        public bool SaveGame()
        {
            return _saveGameService != null
                && _saveGameService.TrySave(Player.Instance, InventoryController.Instance, QuestManager.Instance);
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

        /// <summary>Fast travel: loading screen, then the player is placed on the given spawn point.</summary>
        public void TravelTo(string sceneName, string spawnId)
        {
            if (_isTraveling || Player.Instance == null)
                return;

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarning($"TravelTo: scene '{sceneName}' is not in Build Settings.");
                return;
            }

            StartCoroutine(TravelRoutine(sceneName, spawnId));
        }

        private IEnumerator TravelRoutine(string sceneName, string spawnId)
        {
            _isTraveling = true;
            Player.Instance.CurrentMapId = sceneName;

            yield return SceneTransitionService.LoadSceneWithTransition(
                sceneName, () => PlaceAfterTravel(spawnId));

            _isTraveling = false;
        }

        private IEnumerator PlaceAfterTravel(string spawnId)
        {
            yield return RestoreSceneStateAfterLoad();
            TeleportManager.MovePlayerToSpawn(spawnId);

            // One frame so the camera cut is applied while the loading screen is still showing.
            yield return null;
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

        // Dev/test: Play directly from any map scene (no GameStart, no loading screen).
        private void StartInCurrentScene()
        {
            Player player = Player.Instance;
            if (player == null)
            {
                Debug.LogWarning("StartInCurrentScene: no Player found.");
                return;
            }

            _autoSaveEnabled = false;   // keep the real save untouched while testing one scene

            bool restored = _gameLoadService.TryRestoreFromSave(player, InventoryController.Instance, QuestManager.Instance);
            if (!restored) player.Gold = _newGameGold;

            Scene scene = SceneManager.GetActiveScene();
            player.CurrentMapId = scene.name;
            _sceneStateService.LoadSceneState(scene.name);
            TeleportManager.MovePlayerToSpawn(_newGameSpawnId);
        }

        private void OnApplicationQuit()
        {
            if (Instance != this || !_autoSaveEnabled) return;
            _sceneStateService?.SaveSceneState(SceneManager.GetActiveScene().name);
            SaveGame();
        }

        private void OnApplicationPause(bool paused)   // cho mobile
        {
            if (paused && Instance == this && _autoSaveEnabled)
                SaveGame();
        }
    }
}
