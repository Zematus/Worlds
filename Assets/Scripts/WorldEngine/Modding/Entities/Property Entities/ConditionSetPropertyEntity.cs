using System;

public class ConditionSetPropertyEntity : PropertyEntity<bool>
{
    public IValueExpression<bool>[] Conditions;

    public ConditionSetPropertyEntity(
        Context context, Context.LoadedContext.LoadedProperty p)
        : base(context, p)
    {
        if (p.conditions == null)
        {
            throw new ArgumentException("'conditions' list can't be empty");
        }

        Conditions = ValueExpressionBuilder.BuildValueExpressions<bool>(context, p.conditions);
    }

    public override bool GetValue()
    {
        EvaluateIfNeeded();

        return Value;
    }

    protected override void Calculate()
    {
        Value = true;

        foreach (IValueExpression<bool> exp in Conditions)
        {
            Value &= exp.Value;

            if (!Value)
                break;
        }
    }

    public override string GetFormattedString()
    {
        return GetValue().ToString();
    }
}
