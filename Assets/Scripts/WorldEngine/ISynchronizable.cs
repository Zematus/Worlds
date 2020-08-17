
/// <summary>
/// Synchronizables are serializable objects that need to be synchronized before serialization,
/// and also need to be initialized after being deserialized
/// </summary>
public interface ISynchronizable
{
    /// <summary>
    /// Synchronize all properties that need to be serialized with the current state of the world
    /// </summary>
    void Synchronize();
    /// <summary>
    /// Finish the initialization of the object after being deserialized
    /// </summary>
    void FinalizeLoad();
}
