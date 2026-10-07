using System.Collections.Generic;
using UnityEngine;

public sealed class SceneStateService
{
    private readonly Dictionary<string, Dictionary<string, ISaveable>> _registries =
        new Dictionary<string, Dictionary<string, ISaveable>>();

    public void Register(ISaveable obj, string sceneName)
    {
        if (obj == null || string.IsNullOrEmpty(sceneName))
            return;

        if (!_registries.TryGetValue(sceneName, out Dictionary<string, ISaveable> registry))
        {
            registry = new Dictionary<string, ISaveable>();
            _registries[sceneName] = registry;
        }

        string id = obj.GetID();
        if (!registry.ContainsKey(id))
            registry.Add(id, obj);
    }

    public void Unregister(ISaveable obj, string sceneName)
    {
        if (obj == null || string.IsNullOrEmpty(sceneName))
            return;

        if (!_registries.TryGetValue(sceneName, out Dictionary<string, ISaveable> registry))
            return;

        registry.Remove(obj.GetID());
    }

    public void SaveSceneState(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        if (!_registries.TryGetValue(sceneName, out Dictionary<string, ISaveable> registry) || registry.Count == 0)
            return;

        var data = new SceneData();

        foreach (KeyValuePair<string, ISaveable> pair in registry)
        {
            object captured = pair.Value.CaptureState();
            string json = JsonUtility.ToJson(captured);
            data.entries.Add(new SaveEntry
            {
                id = pair.Key,
                json = json
            });
        }

        SaveSystem.SaveScene(sceneName, data);
    }

    public void LoadSceneState(string sceneName)
    {
        Debug.Log(sceneName);
        if (string.IsNullOrEmpty(sceneName))
            return;

        if (!_registries.TryGetValue(sceneName, out Dictionary<string, ISaveable> registry) || registry.Count == 0)
            return;

        SceneData data = SaveSystem.LoadScene(sceneName);
        if (data?.entries == null || data.entries.Count == 0)
            return;

        foreach (SaveEntry entry in data.entries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.id))
                continue;

            if (registry.TryGetValue(entry.id, out ISaveable saveable))
                saveable.RestoreState(entry.json);
        }
    }

    public void ClearRegistry(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        _registries.Remove(sceneName);
    }
}
