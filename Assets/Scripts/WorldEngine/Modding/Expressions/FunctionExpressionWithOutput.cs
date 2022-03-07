using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class FunctionExpressionWithOutput<T> :
    FunctionExpression, IValueExpression<T>
{
    public FunctionExpressionWithOutput(
        Context c, string id, int minArguments, IExpression[] arguments) :
        base(c, id, minArguments, arguments)
    {
    }

    public abstract T Value { get; }

    public object ValueObject => Value;

    public string GetFormattedString() => Value.ToFormattedString();

    public override string ToPartiallyEvaluatedString(int depth = -1)
    {
        if (depth == 0)
            return Value.ToString();

        depth = (depth > 0) ? depth - 1 : depth;

        return base.ToPartiallyEvaluatedString(depth);
    }
}
