
/// <summary>
/// Base interface for all mod expressions
/// </summary>
public interface IExpression : IInputRequester
{
    string ToPartiallyEvaluatedString(bool evaluate = true);
}
