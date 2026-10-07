using System;
using UnityEngine;
using UnityEngine.Serialization;
using Game.Features.GameFlow;

namespace Game.Features.Save
{
    /// <summary>Base for scene objects whose state is persisted per scene. Registers itself with GameManager.</summary>
    public abstract class SaveableBehaviour : MonoBehaviour, ISaveable
    {
        [FormerlySerializedAs("id")]
        [SerializeField] private string _id;

        public string GetID() => _id;

        public abstract string CaptureState();

        public abstract void RestoreState(string json);

        protected virtual void OnEnable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.Register(this, gameObject.scene.name);
        }

        protected virtual void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.Unregister(this, gameObject.scene.name);
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
