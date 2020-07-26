using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class UnaryOpCellCondition : CellCondition
{
    public Condition Condition;

    public UnaryOpCellCondition(string conditionStr)
    {
        Condition = BuildCondition(conditionStr);

        ConditionType |= Condition.ConditionType;
    }

    public override string GetPropertyValue(string propertyId)
    {
        return Condition.GetPropertyValue(propertyId);
    }
}
