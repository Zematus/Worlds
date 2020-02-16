using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class EntityEqualsExpression : EqualsExpression
{
    private IEntityExpression _entExpressionA;
    private IEntityExpression _entExpressionB;

    protected EntityEqualsExpression(IEntityExpression expressionA, IEntityExpression expressionB) :
        base(expressionA, expressionB)
    {
        _entExpressionA = ExpressionBuilder.ValidateEntityExpression(expressionA);
        _entExpressionB = ExpressionBuilder.ValidateEntityExpression(expressionB);
    }

    public static IExpression Build(IEntityExpression expressionA, IEntityExpression expressionB)
    {
        return new EntityEqualsExpression(expressionA, expressionB);
    }

    public override bool Value => _entExpressionA.Entity == _entExpressionB.Entity;
}
