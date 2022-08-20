
public interface IEffectExpression : IExpression
{
    IEffectTrigger Trigger { get; set; }

    void Apply();
}
