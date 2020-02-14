using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class StringEntityAttributeExpression : IStringExpression
{
    private readonly string _entityExpr;
    private readonly string _attributeId;
    private readonly string _arguments;
    private readonly StringEntityAttribute _attribute;

    public StringEntityAttributeExpression(
        EntityAttribute attribute, string entityExpr, string attrId, string args)
    {
        _attribute = attribute as StringEntityAttribute;
        _entityExpr = entityExpr;
        _attributeId = attrId;
        _arguments = args;
    }

    public override string ToString()
    {
        return _entityExpr + "." + _attributeId
            + (string.IsNullOrWhiteSpace(_arguments) ? "" : "(" + _arguments + ")");
    }

    public string Value => _attribute.GetValue();
}
