using UnityEngine;
using System.Collections.Generic;

public abstract class Subcontext : Context
{
    public Subcontext(Context context) : base(context)
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

    public override int GetNextRandomInt(int iterOffset, int maxValue)
    {
        return _parentContext.GetNextRandomInt(iterOffset, maxValue);
    }

    public override int GetBaseOffset()
    {
        return _parentContext.GetBaseOffset();
    }
}
