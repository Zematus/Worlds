using System.Collections.Generic;
using UnityEngine;

public class FactionSelectionRequest : EntitySelectionRequest<Faction>, IMapEntitySelectionRequest
{
    private readonly HashSet<Faction> _involvedFactions = null;

    public FactionSelectionRequest(
        ICollection<Faction> collection,
        ModText text) :
        base(collection, text)
    {

        _involvedFactions = new HashSet<Faction>();

        foreach (var faction in collection)
        {
            _involvedFactions.Add(faction);
            faction.AssignedFilterType = Faction.FilterType.Selectable;
        }
    }

    public RectInt GetEncompassingRectangle()
    {
        RectInt rect = new RectInt();

        int worldWidth = Manager.CurrentWorld.Width;

        bool first = true;
        foreach (var faction in _involvedFactions)
        {
            RectInt rRect = faction.GetBoundingRectangle();

            if (first)
            {
                rect.SetMinMax(rRect.min, rRect.max);

                first = false;
                continue;
            }

            rect.Extend(rRect, worldWidth);
        }

        return rect;
    }
}
