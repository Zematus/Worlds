using System.Collections.Generic;
using UnityEngine;

public class ContactSelectionRequest : EntitySelectionRequest<PolityContact>, IMapEntitySelectionRequest
{
    private readonly HashSet<Territory> _involvedterritories = null;

    public ContactSelectionRequest(
        ICollection<PolityContact> collection,
        ModText text) :
        base(collection, text)
    {
        Faction guidedFaction = Manager.CurrentWorld.GuidedFaction;

        if (guidedFaction == null)
        {
            throw new System.Exception("Can't create request without an active guided faction");
        }

        Polity guidedPolity = guidedFaction.Polity;

        _involvedterritories = new HashSet<Territory>();

        _involvedterritories.Add(guidedPolity.Territory);
        guidedPolity.Territory.AssignedFilterType = Territory.FilterType.Core;

        foreach (var contact in collection)
        {
            var territory = contact.NeighborPolity.Territory;

            _involvedterritories.Add(territory);
            territory.AssignedFilterType = Territory.FilterType.Selectable;
        }
    }

    public override void Close()
    {
        foreach (var territory in _involvedterritories)
        {
            territory.AssignedFilterType = Territory.FilterType.None;
        }

        base.Close();
    }

    public RectInt GetEncompassingRectangle()
    {
        RectInt rect = new RectInt();

        int worldWidth = Manager.CurrentWorld.Width;

        bool first = true;
        foreach (var territory in _involvedterritories)
        {
            RectInt rRect = territory.GetBoundingRectangle();

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
