using UnityEngine;
using System.Collections.Generic;

public class Description : Context
{
    /// <summary>
    /// String Id for this description
    /// </summary>
    public string Id;

    public ModText Text;

    public Description(Context context) : base(context)
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
}
