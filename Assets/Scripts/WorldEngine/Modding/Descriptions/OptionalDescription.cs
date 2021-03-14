using UnityEngine;
using System.Collections.Generic;

public class OptionalDescription : Description
{
    /// <summary>
    /// Conditions that decide if this description segment should be shown
    /// </summary>
    public IValueExpression<bool>[] Conditions;

    public OptionalDescription(Context context) : base(context)
    {
    }

    public bool CanShow()
    {
        OpenDebugOutput("Evaluation if option '" + Id + "' can be shown:");

        if (Conditions == null)
        {
            CloseDebugOutput("No Conditions. Eval Result: True");
            return true;
        }

        foreach (var exp in Conditions)
        {
            AddExpDebugOutput("Condition", exp);

            if (!exp.Value)
            {
                CloseDebugOutput("Eval Result: False");
                return false;
            }
        }

        CloseDebugOutput("Eval Result: True");
        return true;
    }
}
