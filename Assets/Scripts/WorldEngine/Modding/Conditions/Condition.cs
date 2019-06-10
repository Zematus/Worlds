using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class Condition
{
    public static Condition BuildCondition(string conditionStr)
    {
        Condition condition = null;

        return condition;
    }

    public abstract bool Evaluate(CellGroup group);
    public abstract bool Evaluate(TerrainCell cell);
}
