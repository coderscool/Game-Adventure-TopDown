using System;
using System.IO;
using UnityEngine;
using Game.Features.Save.Data;
using Game.Features.Save.Interfaces;

namespace Game.Features.Save.Services
{
    public sealed class SaveSystem : ISaveStorage
    {
        private const string SaveFileName = "player.json";

        public string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public bool SaveExists()
        {
            return File.Exists(SavePath);
        }

        public string GetScenePath(string sceneName)
        {
            return Path.Combine(Application.persistentDataPath, $"{sceneName}.json");
        }

        public bool TrySave(GameData data)
        {
            if (data == null)
            {
                Debug.LogError("Save failed: GameData is null.");
                return false;
            }

            try
            {
                File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
                Debug.Log($"Saved game to: {SavePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Save failed: {e.Message}");
                return false;
            }
        }

        public bool TryLoad(out GameData data)
        {
            data = null;

            if (!SaveExists())
                return false;

            try
            {
                data = JsonUtility.FromJson<GameData>(File.ReadAllText(SavePath));

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

        public void SaveScene(string sceneName, SceneData data)
        {
            if (string.IsNullOrEmpty(sceneName))
                return;

            try
            {
                File.WriteAllText(GetScenePath(sceneName), JsonUtility.ToJson(data, true));
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveScene failed ({sceneName}): {e.Message}");
            }
        }

        public SceneData LoadScene(string sceneName)
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
    }
}
