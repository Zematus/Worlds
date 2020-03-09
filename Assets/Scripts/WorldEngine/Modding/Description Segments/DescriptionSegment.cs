using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Object that generates events of a certain type during the simulation run
/// </summary>
public class DescriptionSegment : Context
{
    /// <summary>
    /// String Id for this description segment
    /// </summary>
    public string Id;

    public string Text;

    /// <summary>
    /// Conditions that decide if this description segment should be shown
    /// </summary>
    public IBooleanExpression[] Conditions;

    public DescriptionSegment(Context context) : base(context)
    {
        if (context == null)
        {
            throw new System.ArgumentNullException("context can't be null");
        }
    }

    public override float GetNextRandomFloat(int iterOffset)
    {
        return _parentContext.GetNextRandomFloat(iterOffset);
    }

    public override float GetNextRandomInt(int iterOffset, int maxValue)
    {
        return _parentContext.GetNextRandomInt(iterOffset, maxValue);
    }

    protected bool CanShow()
    {
        foreach (IBooleanExpression exp in Conditions)
        {
            if (!exp.Value)
                return false;
        }

        return true;
    }
}
