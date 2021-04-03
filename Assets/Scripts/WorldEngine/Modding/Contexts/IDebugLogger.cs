
public interface IDebugLogger
{
    void OpenDebugOutput(string message);
    void AddDebugOutput(string message);
    void CloseDebugOutput(string message);
}
