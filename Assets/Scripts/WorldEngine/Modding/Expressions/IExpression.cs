
/// <summary>
/// Base interface for all mod expressions
/// </summary>
public interface IExpression : IInputRequester
{
    string ToPartiallyEvaluatedString(int depth = -1);
}
