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
        if (Conditions == null)
        {
            return true;
        }

        foreach (IValueExpression<bool> exp in Conditions)
        {
            if (!exp.Value)
                return false;
        }

        return true;
    }
}
