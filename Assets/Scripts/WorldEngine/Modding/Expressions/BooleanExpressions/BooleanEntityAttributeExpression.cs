using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class BooleanEntityAttributeExpression : BooleanExpression
{
    private readonly string _expressionStr;
    private readonly string _attributeId;
    private readonly BooleanEntityAttribute _attribute;

    public BooleanEntityAttributeExpression(EntityAttribute attribute, string expStr, string attrId)
    {
        _attribute = attribute as BooleanEntityAttribute;
        _expressionStr = expStr;
        _attributeId = attrId;
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
        return _expressionStr + "." + _attributeId;
    }
}
