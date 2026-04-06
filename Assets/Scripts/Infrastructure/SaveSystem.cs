using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private const string SaveFileName = "player.json";

    public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public static bool SaveExists()
    {
        return File.Exists(SavePath);
    }

    public static string GetScenePath(string sceneName)
    {
        return Path.Combine(Application.persistentDataPath, $"{sceneName}.json");
    }

    public static GameData BuildGameData(Player player)
    {
        if (player == null)
            throw new ArgumentNullException(nameof(player));

        InventoryItemData[] inventory = InventoryController.Instance != null
            ? InventoryController.Instance.GetSaveData()
            : new InventoryItemData[0];

        return new GameData
        {
            player = new PlayerData(player),
            inventory = inventory
        };
    }

    public static void SaveScene(string sceneName, SceneData data)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        try
        {
            string path = GetScenePath(sceneName);
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveScene failed ({sceneName}): {e.Message}");
        }
    }

    public static SceneData LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return null;

        string path = GetScenePath(sceneName);
        if (!File.Exists(path))
            return null;

        try
        {
            return JsonUtility.FromJson<SceneData>(File.ReadAllText(path));
        }
        catch (Exception e)
        {
            Debug.LogError($"LoadScene failed ({sceneName}): {e.Message}");
            return null;
        }
    }

    public static bool TrySave(Player player)
    {
        if (player == null)
        {
            Debug.LogError("Save failed: Player is null.");
            return false;
        }

        try
        {
            GameData data = BuildGameData(player);
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"Saved game to: {SavePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Save failed: {e.Message}");
            return false;
        }
    }

    public static bool TryLoad(out GameData data)
    {
        data = null;

        if (!SaveExists())
            return false;

        try
        {
            string json = File.ReadAllText(SavePath);
            data = JsonUtility.FromJson<GameData>(json);

            if (data?.player == null)
            {
                data = null;
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Load failed: {e.Message}");
            data = null;
            return false;
        }
    }
}
