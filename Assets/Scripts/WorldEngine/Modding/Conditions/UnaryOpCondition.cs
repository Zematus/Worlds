using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class UnaryOpCondition : Condition
{
    public Condition Condition;

    public UnaryOpCondition(string conditionStr)
    {
        Condition = BuildCondition(conditionStr);

        ConditionType = Condition.ConditionType;
    }

    public override string GetPropertyValue(string propertyId)
    {
        return Condition.GetPropertyValue(propertyId);
    }
}
