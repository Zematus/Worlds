
public interface IInputRequester
{
    bool RequiresInput { get; }

    bool TryGetRequest(out InputRequest request);
}
