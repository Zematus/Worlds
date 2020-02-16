using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class EntityAttributeExpression : IExpression
{
    private readonly string _arguments;
    private readonly EntityAttribute _attribute;

    public EntityAttributeExpression(
        EntityAttribute attribute, string args)
    {
        _attribute = attribute;
        _arguments = args;
    }

    public override string ToString()
    {
        return _attribute.Entity.Id + "." + _attribute.Id
            + (string.IsNullOrWhiteSpace(_arguments) ? "" : "(" + _arguments + ")");
    }
}
