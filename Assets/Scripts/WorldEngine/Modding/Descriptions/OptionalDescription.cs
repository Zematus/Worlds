using UnityEngine;
using System.Collections.Generic;

public class OptionalDescription : Description
{
    /// <summary>
    /// Conditions that decide if this description segment should be shown
    /// </summary>
    public IBooleanExpression[] Conditions;

    public OptionalDescription(Context context) : base(context)
    {
    }

    protected bool CanShow()
    {
        if (Conditions == null)
        {
            return true;
        }

        foreach (IBooleanExpression exp in Conditions)
        {
            if (!exp.Value)
                return false;
        }

        return true;
    }
}
