using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GetProbabilityAdjectiveFunctionExpression : FunctionExpressionWithOutput<string>
{
    public const string FunctionId = "get_probability_adjective";

    private readonly IValueExpression<float> _valueArg;

    public GetProbabilityAdjectiveFunctionExpression(Context c, IExpression[] arguments) :
        base(c, FunctionId, 1, arguments)
    {
        _valueArg = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[0]);
    }

    public override string Value
    {
        get {
            if (_valueArg.Value >= 1)
            {
                return "inevitable";
            }

            if (_valueArg.Value >= 0.95)
            {
                return "extremely likely";
            }

            if (_valueArg.Value >= 0.85)
            {
                return "very likely";
            }

            if (_valueArg.Value >= 0.7)
            {
                return "likely";
            }

            if (_valueArg.Value > 0.3)
            {
                return "possible";
            }

            if (_valueArg.Value > 0.15)
            {
                return "unlikely";
            }

            if (_valueArg.Value > 0.05)
            {
                return "very unlikely";
            }

            if (_valueArg.Value > 0.0)
            {
                return "extremely unlikely";
            }

            if (_valueArg.Value <= 0.0)
            {
                return "impossible";
            }

            throw new System.ArgumentException(
                $"{_context.Id} - {FunctionId}: couldn't find probability adjective" +
                $"\n - expression: {ToString()}" +
                $"\n - input value: {_valueArg.Value}");
        }
    }
}
