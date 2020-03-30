using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SplitFactionExpression : IEffectExpression
{
    public const string FunctionId = "split_faction";

    private readonly IValueExpression<Entity> _factionArg;
    private readonly IValueExpression<Entity> _coreGroupArg;
    private readonly IValueExpression<float> _influenceTransferArg;
    private readonly IValueExpression<float> _relationshipValueArg;

    public SplitFactionExpression(IExpression[] arguments)
    {
        if ((arguments == null) || (arguments.Length < 4))
        {
            throw new System.ArgumentException("Number of arguments less than 2");
        }

        _factionArg =
            ValueExpressionBuilder.ValidateValueExpression<Entity>(arguments[0]);
        _coreGroupArg =
            ValueExpressionBuilder.ValidateValueExpression<Entity>(arguments[1]);
        _influenceTransferArg =
            ValueExpressionBuilder.ValidateValueExpression<float>(arguments[2]);
        _relationshipValueArg =
            ValueExpressionBuilder.ValidateValueExpression<float>(arguments[3]);
    }

    public void Apply()
    {
        throw new System.NotImplementedException();
    }

    public override string ToString()
    {
        throw new System.NotImplementedException();

        return "split_faction(" +
            _factionArg.ToString() + ", " +
            _influenceTransferArg.ToString() + ")";
    }
}
