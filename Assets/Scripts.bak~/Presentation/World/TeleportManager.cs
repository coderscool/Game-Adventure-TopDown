using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance;

    private string targetScene;
    private string targetSpawnID;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Teleport(string sceneName, string spawnID)
    {
        targetScene = sceneName;
        targetSpawnID = spawnID;

        StartCoroutine(TeleportRoutine());
    }

    IEnumerator TeleportRoutine()
    {
        // fade out
        //yield return FadeUI.Instance.FadeOut();

        yield return SceneManager.LoadSceneAsync(targetScene);

        yield return null; // đợi 1 frame

        SpawnPlayer();

        // fade in
        //yield return FadeUI.Instance.FadeIn();
    }

    void SpawnPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("Không tìm thấy Player trong scene");
            return;
        }

        SpawnPoint[] spawns = GameObject.FindObjectsOfType<SpawnPoint>();

        if (spawns.Length == 0)
        {
            Debug.LogError("Không có SpawnPoint trong scene");
            return;
        }

        foreach (var sp in spawns)
        {
            if (sp.spawnID == targetSpawnID)
            {
                player.transform.position = sp.transform.position;
                return;
            }
        }

        Debug.LogWarning("Không tìm thấy SpawnPoint: " + targetSpawnID);
    }
}