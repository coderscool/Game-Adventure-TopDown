using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Features.Save
{
    /// <summary>
    /// Base for scene objects whose state is persisted per scene.
    /// Talks to the outside world only through static events, so the Save feature does not depend on who listens.
    /// </summary>
    public abstract class SaveableBehaviour : MonoBehaviour, ISaveable
    {
        /// <summary>Raised when a saveable becomes active. Args: object, scene name.</summary>
        public static event Action<ISaveable, string> Registered;

        /// <summary>Raised when a saveable is destroyed. Args: object, scene name.</summary>
        public static event Action<ISaveable, string> Unregistered;

        /// <summary>Raised when a saveable asks to persist its whole scene. Arg: scene name.</summary>
        public static event Action<string> SceneSaveRequested;

        [FormerlySerializedAs("id")]
        [SerializeField] private string _id;

        public string GetID() => _id;

        public abstract string CaptureState();

        public abstract void RestoreState(string json);

        protected void RequestSceneSave()
        {
            SceneSaveRequested?.Invoke(gameObject.scene.name);
        }

        protected virtual void OnEnable()
        {
            Registered?.Invoke(this, gameObject.scene.name);
        }

        protected virtual void OnDestroy()
        {
            Unregistered?.Invoke(this, gameObject.scene.name);
        }

        // Ids are generated in the editor (on add / when empty) so they are stable and saved with the scene.
        // Duplicated objects copy the id: use the context menu to give the copy a new one.
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(_id))
                AssignNewId();
        }

        [ContextMenu("Regenerate Save Id")]
        private void AssignNewId()
        {
            _id = Guid.NewGuid().ToString();
    #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
    #endif
        }
    }
}
