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
