using Game.Features.Save.Data;

namespace Game.Features.Save.Interfaces
{
    /// <summary>Persistence port. Implemented by the Infrastructure layer.</summary>
    public interface ISaveStorage
    {
        bool TrySave(GameData data);
        bool TryLoad(out GameData data);

        void SaveScene(string sceneName, SceneData data);
        SceneData LoadScene(string sceneName);
    }
}
