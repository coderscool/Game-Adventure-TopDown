namespace Game.Features.Save
{
    public interface ISaveable
    {
        string GetID();

        /// <summary>Returns the state serialized as JSON.</summary>
        string CaptureState();

        void RestoreState(string json);
    }
}
