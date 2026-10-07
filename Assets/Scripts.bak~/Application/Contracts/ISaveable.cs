public interface ISaveable
{
    string GetID();
    object CaptureState();
    void RestoreState(object state);
}
