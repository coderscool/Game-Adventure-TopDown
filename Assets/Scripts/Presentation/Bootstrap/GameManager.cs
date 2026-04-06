using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Animator transition;

    public float transitionTime = 1f;

    private SceneTransitionService sceneTransitionService;
    private GameLoadService gameLoadService;
    private SceneStateService sceneStateService;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sceneTransitionService = new SceneTransitionService(transition, transitionTime);
        gameLoadService = new GameLoadService();
        sceneStateService = new SceneStateService();
    }

    void Start()
    {
        StartCoroutine(LoadSceneRoutine(Player.Instance));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            LoadScene();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public void SaveSceneState(string sceneName)
    {
        sceneStateService?.SaveSceneState(sceneName);
    }

    public void LoadSceneState(string sceneName)
    {
        sceneStateService?.LoadSceneState(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //LoadSceneState(scene.name);
    }

    private void OnSceneUnloaded(Scene scene)
    {
        sceneStateService?.SaveSceneState(scene.name);
        sceneStateService?.ClearRegistry(scene.name);
    }

    public void Register(ISaveable obj, string sceneName)
    {
        sceneStateService?.Register(obj, sceneName);
    }

    public void Unregister(ISaveable obj, string sceneName)
    {
        sceneStateService?.Unregister(obj, sceneName);
    }

    IEnumerator AfterSceneLoaded()
    {
        // Load player data
        yield return gameLoadService.TryRestoreFromSave();

        yield return null;

        // Restore object trong scene
        LoadSceneState(SceneManager.GetActiveScene().name);

        yield return new WaitForSeconds(0.1f);
    }

    public void LoadScene()
    {
        StartCoroutine(LoadSceneRoutine(Player.Instance));
    }

    IEnumerator LoadSceneRoutine(Player player)
    {
        Debug.Log(player.currentMapId);
        yield return sceneTransitionService.LoadSceneWithTransition(
            player.currentMapId,
            AfterSceneLoaded
        );
    }

    public void SaveGame()
    {
        SaveGameService.TrySaveCurrentPlayer();
    }
}
