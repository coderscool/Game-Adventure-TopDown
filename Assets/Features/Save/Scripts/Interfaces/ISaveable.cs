namespace Game.Features.Save.Interfaces
{
    public interface ISaveable
    {
        string GetID();

        /// <summary>Returns the state serialized as JSON.</summary>
        string CaptureState();

        void RestoreState(string json);
    }
}
