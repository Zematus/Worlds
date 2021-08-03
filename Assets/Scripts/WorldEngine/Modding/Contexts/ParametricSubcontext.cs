using UnityEngine;
using System.Collections.Generic;

public class ParametricSubcontext : Context
{
    public ParametricSubcontext(string id, Context context) : base(context)
    {
        if (context == null)
        {
            throw new System.ArgumentNullException("context can't be null");
        }

        Id = id;
    }

    public Entity GetEntity(string id)
    {
        if (!_entities.TryGetValue(id, out Entity entity))
        {
            throw new System.Exception($"Parameter entity with id '{id}' not defined within the '{Id}' subcontext");
        }

        return entity;
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
