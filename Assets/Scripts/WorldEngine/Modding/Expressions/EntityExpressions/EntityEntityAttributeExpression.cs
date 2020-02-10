using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public class EntityEntityAttributeExpression : EntityExpression
{
    private readonly string _expressionStr;
    private readonly string _attributeId;
    private readonly string _arguments;
    private readonly EntityEntityAttribute _attribute;

    public EntityEntityAttributeExpression(
        EntityAttribute attribute, string expStr, string attrId, string args)
    {
        _attribute = attribute as EntityEntityAttribute;
        _expressionStr = expStr;
        _attributeId = attrId;
        _arguments = args;
    }

    public override Entity GetEntity()
    {
        return _attribute.GetEntity();
    }

    public override void Reset()
    {
        _attribute.Reset();

        base.Reset();
    }

    public override string ToString()
    {
        return _expressionStr + "." + _attributeId
            + (string.IsNullOrWhiteSpace(_arguments) ? "" : "(" + _arguments + ")");
    }
}
