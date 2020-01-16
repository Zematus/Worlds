using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class OrCondition : BinaryOpCondition
{
    public OrCondition(string conditionAStr, string conditionBStr) : base(conditionAStr, conditionBStr)
    {
    }

    public override bool Evaluate(CellGroup group)
    {
        return ConditionA.Evaluate(group) || ConditionB.Evaluate(group);
    }

    public override bool Evaluate(TerrainCell cell)
    {
        return ConditionA.Evaluate(cell) || ConditionB.Evaluate(cell);
    }

    public override string ToString()
    {
        return "(" + ConditionA.ToString() + ") OR (" + ConditionB.ToString() + ")";
    }
}
