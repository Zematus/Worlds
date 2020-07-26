using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class OrExpression : BinaryOpExpressionWithOutput<bool>
{
    private readonly IValueExpression<bool> _boolExpressionA;
    private readonly IValueExpression<bool> _boolExpressionB;

    public OrExpression(
        IValueExpression<bool> expressionA,
        IValueExpression<bool> expressionB) :
        base("||", expressionA, expressionB)
    {
        _boolExpressionA = expressionA;
        _boolExpressionB = expressionB;
    }

    public static IExpression Build(Context context, string expressionAStr, string expressionBStr)
    {
        IValueExpression<bool> expressionA =
            ValueExpressionBuilder.BuildValueExpression<bool>(context, expressionAStr);
        IValueExpression<bool> expressionB =
            ValueExpressionBuilder.BuildValueExpression<bool>(context, expressionBStr);

        return new OrExpression(expressionA, expressionB);
    }

    public override bool Value => _boolExpressionA.Value || _boolExpressionB.Value;
}
