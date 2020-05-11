using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class EntityAttributeExpression : IExpression
{
    private readonly EntityAttribute _attribute;

    public EntityAttributeExpression(EntityAttribute attribute)
    {
        _attribute = attribute;
    }

    public string ToPartiallyEvaluatedString(bool evaluate)
    {
        if (_attribute.Arguments == null)
        {
            return _attribute.ToPartiallyEvaluatedString(evaluate);
        }

        string output = _attribute.Entity.Id + "." + _attribute.Id + "(";

        bool notFirst = true;
        foreach (IExpression argument in _attribute.Arguments)
        {
            if (notFirst)
            {
                output += ", ";
            }

            output += argument.ToPartiallyEvaluatedString(true);
            notFirst = true;
        }

        output += ")";

        return output;
    }

    public override string ToString()
    {
        string output = _attribute.Entity.Id + "." + _attribute.Id;

        if (_attribute.Arguments == null)
        {
            return output;
        }

        output += "(";
        output += string.Join<IExpression>(", ", _attribute.Arguments);
        output += ")";

        return output;
    }
}
