using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class UnaryOpGroupCondition : GroupCondition
{
    public Condition Condition;

    public UnaryOpGroupCondition(string conditionStr)
    {
        Condition = BuildCondition(conditionStr);

        ConditionType |= Condition.ConditionType;
    }

    public override string GetPropertyValue(string propertyId)
    {
        return Condition.GetPropertyValue(propertyId);
    }
}
