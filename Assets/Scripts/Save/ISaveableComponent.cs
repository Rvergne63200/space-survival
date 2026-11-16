// A component whose state is saved with its SaveableObject (chest content, pickup count...)
public interface ISaveableComponent
{
    // Identifies the component in the saved object: must be unique among the saveable components of the object
    string SaveKey { get; }

    // State as JSON (JsonUtility)
    string CaptureState();

    void RestoreState(string json);
}
