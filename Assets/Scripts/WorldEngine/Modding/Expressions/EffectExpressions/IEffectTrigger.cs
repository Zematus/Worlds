
public interface IEffectTrigger
{
#if DEBUG
    long GetLastUseDate(IEffectExpression expression);
    void SetLastUseDate(IEffectExpression expression, long date);
#endif
}
