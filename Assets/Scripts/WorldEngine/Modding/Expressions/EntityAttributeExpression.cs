using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public abstract class EntityAttributeExpression : IExpression
{
    private readonly EntityAttribute _attribute;

    public EntityAttributeExpression(EntityAttribute attribute)
    {
        _attribute = attribute;
    }

    public bool RequiresInput => _attribute.RequiresInput;

    public string ToPartiallyEvaluatedString(int depth = -1)
    {
        if ((_attribute.Arguments == null) || (depth == 0))
        {
            return _attribute.ToPartiallyEvaluatedString(depth);
        }

        depth = (depth > 0) ? depth - 1 : depth;

        string output = $"{_attribute.Entity.Id}.{_attribute.Id}(" +
            string.Join(
                ", ", _attribute.Arguments.Select(e => e.ToPartiallyEvaluatedString(depth))) +
            $")";

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

    public bool TryGetRequest(out InputRequest request) =>
        _attribute.TryGetRequest(out request);
}
