using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class BooleanEntityAttributeExpression : BooleanExpression
{
    private readonly string _expressionStr;
    private readonly string _attributeId;
    private readonly string _arguments;
    private readonly BooleanEntityAttribute _attribute;

    public BooleanEntityAttributeExpression(
        EntityAttribute attribute, string expStr, string attrId, string args)
    {
        _attribute = attribute as BooleanEntityAttribute;
        _expressionStr = expStr;
        _attributeId = attrId;
        _arguments = args;
    }

    protected override bool Evaluate()
    {
        return _attribute.GetValue();
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
