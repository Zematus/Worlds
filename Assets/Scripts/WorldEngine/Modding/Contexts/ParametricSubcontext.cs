using UnityEngine;
using System.Collections.Generic;

public class ParametricSubcontext : Subcontext
{
    public ParametricSubcontext(string id, Context context) : base(context)
    {
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
}
