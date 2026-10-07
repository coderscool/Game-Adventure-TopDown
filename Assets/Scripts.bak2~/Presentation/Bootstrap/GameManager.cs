using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        // Composition root: the only place that knows the concrete storage.
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
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public void Register(ISaveable obj, string sceneName)
    {
        _sceneStateService?.Register(obj, sceneName);
    }

    public void Unregister(ISaveable obj, string sceneName)
    {
        _sceneStateService?.Unregister(obj, sceneName);
    }

    public void SaveSceneState(string sceneName)
    {
        _sceneStateService?.SaveSceneState(sceneName);
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
