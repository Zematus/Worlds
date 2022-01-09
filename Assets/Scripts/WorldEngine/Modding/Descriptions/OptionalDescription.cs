using System;
using System.Collections.Generic;
using UnityEngine;

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
        OpenDebugOutput("Evaluating if option '" + Id + "' is available:");

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
