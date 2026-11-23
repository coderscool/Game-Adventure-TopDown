using Game.Features.GameFlow.Managers;
using UnityEngine;

namespace Game.Features.GameFlow.Systems
{
    /// <summary>
    /// Tự tạo các hệ thống persistent (Player, Canvas, Camera, GameManager...) từ prefab
    /// Resources/Bootstrap khi nhấn Play ở BẤT KỲ scene nào, để test không cần qua GameStart.
    /// </summary>
    public static class GameBootstrap
    {
        private const string PrefabPath = "Bootstrap";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (GameManager.Instance != null) return;

            GameObject prefab = Resources.Load<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"Missing prefab Resources/{PrefabPath}. Create it under Features/GameFlow/Resources.");
                return;
            }

            // Instantiate under an inactive holder, then detach children so each one is a root
            // object (DontDestroyOnLoad only works on roots) and its Awake runs after detaching.
            var holder = new GameObject("BootstrapHolder");
            holder.SetActive(false);
            GameObject instance = Object.Instantiate(prefab, holder.transform);

            Transform root = instance.transform;
            while (root.childCount > 0)
                root.GetChild(0).SetParent(null, true);

            Object.Destroy(holder);
        }
    }
}
