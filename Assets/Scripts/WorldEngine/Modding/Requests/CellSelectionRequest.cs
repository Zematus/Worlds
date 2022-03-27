using System.Collections.Generic;
using UnityEngine;

public class CellSelectionRequest : EntitySelectionRequest<TerrainCell>, IMapEntitySelectionRequest
{
    private readonly HashSet<TerrainCell> _involvedCells = null;

    public CellSelectionRequest(
        ICollection<TerrainCell> collection,
        ModText text) :
        base(collection, text)
    {
        Faction guidedFaction = Manager.CurrentWorld.GuidedFaction;

        if (guidedFaction == null)
        {
            throw new System.Exception("Can't create request without an active guided faction");
        }

        _involvedCells = new HashSet<TerrainCell>(collection)
        {
            guidedFaction.CoreGroup.Cell
        };

        // Set involved groups as filtered so that the UI can quickly filter them

        guidedFaction.CoreGroup.Cell.SelectionFilterType = TerrainCell.FilterType.Core;

        foreach (var cell in collection)
        {
            cell.SelectionFilterType = TerrainCell.FilterType.Selectable;
        }
    }

    public override void Close()
    {
        foreach (var cell in _involvedCells)
        {
            cell.SelectionFilterType = TerrainCell.FilterType.None;
        }

        base.Close();
    }

    /// <summary>
    /// Returns the smallest rectangle that encompasses all selectable cells 
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
        foreach (var cell in _involvedCells)
        {
            if (first)
            {
                rect.SetMinMax(cell.Position, cell.Position);

                first = false;
                continue;
            }

            rect.Extend(cell.Position, worldWidth);
        }

        return rect;
    }
}
