using System.Collections.Generic;
using UnityEngine;

public class RegionSelectionRequest : EntitySelectionRequest<Region>, IMapEntitySelectionRequest
{
    private readonly HashSet<Region> _involvedRegions = null;

    public ModText Text { get; private set; }

    public RegionSelectionRequest(
        ICollection<Region> collection,
        ModText text) :
        base(collection)
    {
        Faction guidedFaction = Manager.CurrentWorld.GuidedFaction;

        if (guidedFaction == null)
        {
            throw new System.Exception("Can't create request without an active guided faction");
        }

        Text = text;

        Polity guidedPolity = guidedFaction.Polity;

        _involvedRegions = new HashSet<Region>(guidedPolity.CoreRegions);
        _involvedRegions.UnionWith(collection);

        // Set involved regions as filtered so that the UI can quickly filter them

        foreach (Region region in guidedPolity.CoreRegions)
        {
            region.AssignedFilterType = Region.FilterType.Core;
        }

        foreach (Region region in collection)
        {
            region.AssignedFilterType = Region.FilterType.Selectable;
        }
    }

    public override void Close()
    {
        foreach (Region region in _involvedRegions)
        {
            region.AssignedFilterType = Region.FilterType.None;
        }

        base.Close();
    }

    /// <summary>
    /// Returns the smallest rectangle that encompasses all selectable regions 
    /// in this request.
    /// NOTE: The rect returned by this function can contain longitude values
    /// that are greater than the current world width.
    /// </summary>
    /// <returns>a rectange with min and max longitude and latitude values</returns>
    public RectInt GetEncompassingRectangle()
    {
        RectInt rect = new RectInt();

        int worldWidth = Manager.CurrentWorld.Width;

        bool first = true;
        foreach (Region region in _involvedRegions)
        {
            RectInt rRect = region.GetBoundingRectangle();

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
