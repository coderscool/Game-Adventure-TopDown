using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    public CinemachineVirtualCamera vcam;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetTarget();
    }

    void SetTarget()
    {
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null && vcam != null)
        {
            vcam.Follow = player.transform;
            vcam.LookAt = player.transform;
        }
        else
        {
            Debug.LogWarning("Player hoặc Camera chưa có!");
        }
    }
}