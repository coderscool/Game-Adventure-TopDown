using System;
using UnityEngine;

public abstract class SaveableBehaviour : MonoBehaviour, ISaveable
{
    [SerializeField] private string id = Guid.NewGuid().ToString();

    public string GetID() => id;

    protected virtual void OnEnable()
    {
        Debug.Log("enabled");
        string sceneName = gameObject.scene.name;
        GameManager.Instance?.Register(this, sceneName);
    }

    protected virtual void OnDestroy()
    {
        GameManager.Instance?.Unregister(this, gameObject.scene.name);
    }

    public abstract object CaptureState();

    public abstract void RestoreState(object state);
}
