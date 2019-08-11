using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AndCondition : BinaryOpCondition
{
    public AndCondition(string conditionAStr, string conditionBStr) : base(conditionAStr, conditionBStr)
    {
    }

    public override bool Evaluate(CellGroup group)
    {
        return ConditionA.Evaluate(group) && ConditionB.Evaluate(group);
    }

    public override bool Evaluate(TerrainCell cell)
    {
        return ConditionA.Evaluate(cell) && ConditionB.Evaluate(cell);
    }

    public override string ToString()
    {
        return "(" + ConditionA.ToString() + ") AND (" + ConditionB.ToString() + ")";
    }
}
