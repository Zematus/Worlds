using System.Collections.Generic;
using UnityEngine;

public class FactionSelectionRequest : EntitySelectionRequest<Faction>, IMapEntitySelectionRequest
{
    public FactionSelectionRequest(
        ICollection<Faction> collection,
        ModText text) :
        base(collection, text)
    {
    }

    public RectInt GetEncompassingRectangle()
    {
        throw new System.NotImplementedException();
    }
}
