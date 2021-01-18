using System;

public class ConditionSetPropertyEntity : PropertyEntity<bool>
{
    public IValueExpression<bool>[] Conditions;

    public override bool RequiresInput
    {
        get
        {
            foreach (IValueExpression<bool> c in Conditions)
            {
                if (c.RequiresInput)
                    return true;
            }

            return false;
        }
    }

    public ConditionSetPropertyEntity(
        Context context, Context.LoadedContext.LoadedProperty p)
        : base(context, p)
    {
        if (p.conditions == null)
        {
            throw new ArgumentException("'conditions' list can't be empty");
        }

        Conditions = ValueExpressionBuilder.BuildValueExpressions<bool>(
            context, p.conditions, true);
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

    public override string GetDebugString()
    {
        return "condition_set:" + GetValue().ToString();
    }

    public override string GetFormattedString()
    {
        return GetValue().ToString().ToBoldFormat();
    }

    public override string ToPartiallyEvaluatedString(bool evaluate)
    {
        string output = "(";

        bool notFirst = false;
        foreach (IValueExpression<bool> exp in Conditions)
        {
            if (notFirst)
            {
                output += " && ";
            }

            output += exp.ToPartiallyEvaluatedString(evaluate);
            notFirst = true;
        }

        return output + ")";
    }

    public override bool TryGetRequest(out InputRequest request)
    {
        foreach (IValueExpression<bool> c in Conditions)
        {
            if (c.TryGetRequest(out request))
                return true;
        }

        request = null;

        return false;
    }
}
