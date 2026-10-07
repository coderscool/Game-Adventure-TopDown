using System.Collections.Generic;
using UnityEngine;

public sealed class SceneStateService
{
    private readonly Dictionary<string, Dictionary<string, ISaveable>> _registries =
        new Dictionary<string, Dictionary<string, ISaveable>>();

    private readonly ISaveStorage _storage;

    public SceneStateService(ISaveStorage storage)
    {
        _storage = storage;
    }

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
        if (!registry.TryGetValue(id, out ISaveable existing))
        {
            registry.Add(id, obj);
            return;
        }

        if (!ReferenceEquals(existing, obj))
            Debug.LogWarning($"Duplicate save id '{id}' in scene '{sceneName}'. Regenerate the id on one of the objects.");
    }

    public void Unregister(ISaveable obj, string sceneName)
    {
        if (obj == null || !TryGetRegistry(sceneName, out Dictionary<string, ISaveable> registry))
            return;

        registry.Remove(obj.GetID());
    }

    public void SaveSceneState(string sceneName)
    {
        if (!TryGetRegistry(sceneName, out Dictionary<string, ISaveable> registry) || registry.Count == 0)
            return;

        var data = new SceneData();

        foreach (KeyValuePair<string, ISaveable> pair in registry)
        {
            data.entries.Add(new SaveEntry
            {
                id = pair.Key,
                json = pair.Value.CaptureState()
            });
        }

        _storage.SaveScene(sceneName, data);
    }

    public void LoadSceneState(string sceneName)
    {
        if (!TryGetRegistry(sceneName, out Dictionary<string, ISaveable> registry) || registry.Count == 0)
            return;

        SceneData data = _storage.LoadScene(sceneName);
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

    private bool TryGetRegistry(string sceneName, out Dictionary<string, ISaveable> registry)
    {
        registry = null;
        return !string.IsNullOrEmpty(sceneName) && _registries.TryGetValue(sceneName, out registry);
    }
}
