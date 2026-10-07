using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance { get; private set; }

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

    public void Teleport(string sceneName, string spawnId)
    {
        StartCoroutine(TeleportRoutine(sceneName, spawnId));
    }

    private IEnumerator TeleportRoutine(string sceneName, string spawnId)
    {
        yield return SceneManager.LoadSceneAsync(sceneName);

        // Chờ 1 frame để object của scene mới khởi tạo xong.
        yield return null;

        MovePlayerToSpawn(spawnId);
    }

    private static void MovePlayerToSpawn(string spawnId)
    {
        Player player = Player.Instance;
        if (player == null)
        {
            Debug.LogError("Không tìm thấy Player trong scene");
            return;
        }

        SpawnPoint[] spawns = FindObjectsOfType<SpawnPoint>();
        if (spawns.Length == 0)
        {
            Debug.LogError("Không có SpawnPoint trong scene");
            return;
        }

        foreach (SpawnPoint spawn in spawns)
        {
            if (spawn.SpawnId != spawnId)
                continue;

            player.transform.position = spawn.transform.position;
            return;
        }

        Debug.LogWarning("Không tìm thấy SpawnPoint: " + spawnId);
    }
}
