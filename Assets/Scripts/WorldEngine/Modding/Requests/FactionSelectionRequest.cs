using System.Collections.Generic;
using UnityEngine;

public class FactionSelectionRequest : EntitySelectionRequest<Faction>, IMapEntitySelectionRequest
{
    private readonly HashSet<Territory> _involvedTerritories = null;
    private readonly HashSet<Faction> _involvedFactions = null;

    public FactionSelectionRequest(
        ICollection<Faction> collection,
        ModText text) :
        base(collection, text)
    {
        _involvedFactions = new HashSet<Faction>();
        _involvedTerritories = new HashSet<Territory>();

        Faction guidedFaction = Manager.CurrentWorld.GuidedFaction;

        if (guidedFaction == null)
        {
            throw new System.Exception("Can't create request without an active guided faction");
        }

        Polity guidedPolity = guidedFaction.Polity;

        _involvedTerritories = new HashSet<Territory>();

        _involvedTerritories.Add(guidedPolity.Territory);
        guidedPolity.Territory.SelectionFilterType = Territory.FilterType.Core;

        foreach (var faction in collection)
        {
            _involvedFactions.Add(faction);
            faction.SelectionFilterType = Faction.FilterType.Selectable;

            var territory = faction.Polity.Territory;

            if (territory.SelectionFilterType == Territory.FilterType.None)
            {
                _involvedTerritories.Add(territory);
                territory.SelectionFilterType = Territory.FilterType.Involved;
            }
        }

        foreach (var territory in _involvedTerritories)
        {
            foreach (var faction in territory.Polity.GetFactions())
            {
                if (faction.SelectionFilterType == Faction.FilterType.None)
                {
                    _involvedFactions.Add(faction);
                    faction.SelectionFilterType = Faction.FilterType.Related;
                }
            }
        }
    }

    public override void Close()
    {
        foreach (var faction in _involvedFactions)
        {
            faction.SelectionFilterType = Faction.FilterType.None;
        }

        foreach (var territory in _involvedTerritories)
        {
            territory.SelectionFilterType = Territory.FilterType.None;
        }

        base.Close();
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
