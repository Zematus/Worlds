using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class BinaryOpCondition : Condition
{
    public Condition ConditionA;
    public Condition ConditionB;

    public BinaryOpCondition(string conditionAStr, string conditionBStr)
    {
        ConditionA = BuildCondition(conditionAStr);
        ConditionB = BuildCondition(conditionBStr);

        ConditionType = ConditionA.ConditionType | ConditionB.ConditionType;
    }

    public override string GetPropertyValue(string propertyId)
    {
        string value = ConditionA.GetPropertyValue(propertyId);
        string valueB = ConditionB.GetPropertyValue(propertyId);

        if (valueB != null)
        {
            if (value != null)
                value += "," + valueB;
            else
                value = valueB;
        }

        return value;
    }
}
