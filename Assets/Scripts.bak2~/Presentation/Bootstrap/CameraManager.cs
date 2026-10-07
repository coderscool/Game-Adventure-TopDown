using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [FormerlySerializedAs("vcam")]
    [SerializeField] private CinemachineVirtualCamera _vcam;

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
        if (Instance == this)
            SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FollowPlayer();
    }

    private void FollowPlayer()
    {
        Player player = Player.Instance;

        if (player == null || _vcam == null)
        {
            Debug.LogWarning("Player hoặc Camera chưa có!");
            return;
        }

        _vcam.Follow = player.transform;
        _vcam.LookAt = player.transform;
    }
}
