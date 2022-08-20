using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

public class Border : CellSet
{
    public int Id;

    public Border(int id, TerrainCell startCell)
    {
        Id = id;

        AddCell(startCell);
    }

    private bool IsSetOutside(HashSet<TerrainCell> set)
    {
        TerrainCell northOfTop = Top.GetNeighborCell(Direction.North);
        if ((northOfTop != null) && set.Contains(northOfTop))
            return true;

        TerrainCell westOfLeft = Left.GetNeighborCell(Direction.West);
        if ((westOfLeft != null) && set.Contains(westOfLeft))
            return true;

        TerrainCell southOfBottom = Bottom.GetNeighborCell(Direction.South);
        if ((southOfBottom != null) && set.Contains(southOfBottom))
            return true;

        TerrainCell eastOfRight = Right.GetNeighborCell(Direction.East);
        if ((eastOfRight != null) && set.Contains(eastOfRight))
            return true;

        return false;
    }

    public bool TryGetEnclosedCellSet(
        HashSet<TerrainCell> outsideSet,
        out CellSet enclosedCellSet,
        CanAddCellDelegate canAddCell = null)
    {
        enclosedCellSet = null;

        // Test if border encloses an area not in outsideSet
        if (!IsSetOutside(outsideSet))
            return false;

        enclosedCellSet = new CellSet();

        var exploredSet = new HashSet<TerrainCell>();
        exploredSet.UnionWith(outsideSet);

        var toAdd = new Queue<TerrainCell>();

        toAdd.Enqueue(Top);

        while (toAdd.Count > 0)
        {
            TerrainCell cell = toAdd.Dequeue();

            if ((canAddCell == null) || canAddCell(cell))
            {
                enclosedCellSet.AddCell(cell);
            }

            if (enclosedCellSet.Area > RectArea)
            {
                throw new System.Exception("Border does not fully enclose inner area");
            }

            foreach (KeyValuePair<Direction, TerrainCell> pair in cell.NonDiagonalNeighbors)
            {
                TerrainCell nCell = pair.Value;

                if (exploredSet.Contains(nCell)) continue;

                if (!IsCellEnclosed(nCell)) continue;

                toAdd.Enqueue(nCell);
                exploredSet.Add(nCell);
            }
        }

        if (enclosedCellSet.Area == 0)
        {
            return false;
        }

        enclosedCellSet.Update();

        return true;
    }

    public void Consolidate(HashSet<TerrainCell> innerArea)
    {
        HashSet<TerrainCell> cellsWithinArea = new HashSet<TerrainCell>();

        foreach (TerrainCell cell in Cells)
        {
            if (innerArea.Contains(cell))
            {
                cellsWithinArea.Add(cell);
            }
        }

        foreach (TerrainCell cell in cellsWithinArea)
        {
            Cells.Remove(cell);
        }

        Update();
    }
}
