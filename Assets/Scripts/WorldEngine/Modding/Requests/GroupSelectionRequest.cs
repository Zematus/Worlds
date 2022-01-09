using System.Collections.Generic;
using UnityEngine;

public class GroupSelectionRequest : EntitySelectionRequest<CellGroup>, IMapEntitySelectionRequest
{
    private readonly HashSet<CellGroup> _involvedGroups = null;

    public GroupSelectionRequest(
        ICollection<CellGroup> collection,
        ModText text) :
        base(collection, text)
    {
        Faction guidedFaction = Manager.CurrentWorld.GuidedFaction;

        if (guidedFaction == null)
        {
            throw new System.Exception("Can't create request without an active guided faction");
        }

        _involvedGroups = new HashSet<CellGroup>(collection);
        _involvedGroups.Add(guidedFaction.CoreGroup);

        // Set involved groups as filtered so that the UI can quickly filter them

        guidedFaction.CoreGroup.Cell.SelectionFilterType = TerrainCell.FilterType.Core;

        foreach (var group in collection)
        {
            group.Cell.SelectionFilterType = TerrainCell.FilterType.Selectable;
        }
    }

    public override void Close()
    {
        foreach (var group in _involvedGroups)
        {
            group.Cell.SelectionFilterType = TerrainCell.FilterType.None;
        }

        base.Close();
    }

    /// <summary>
    /// Returns the smallest rectangle that encompasses all selectable groups 
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
        foreach (CellGroup group in _involvedGroups)
        {
            if (first)
            {
                rect.SetMinMax(group.Position, group.Position);

                first = false;
                continue;
            }

            rect.Extend(group.Position, worldWidth);
        }

        return rect;
    }
}
