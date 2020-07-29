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

    public CellSet GetEnclosedCellSet(HashSet<TerrainCell> outsideSet)
    {
        CellSet cellSet = new CellSet();

        HashSet<TerrainCell> exploredSet = new HashSet<TerrainCell>();
        exploredSet.UnionWith(outsideSet);

        Queue<TerrainCell> toAdd = new Queue<TerrainCell>();

        toAdd.Enqueue(Top);

        while (toAdd.Count > 0)
        {
            TerrainCell cell = toAdd.Dequeue();

            if (!cell.IsLiquidSea)
            {
                cellSet.AddCell(cell);
            }

            if (cellSet.Area > RectArea)
            {
                throw new System.Exception("Border does not fully enclose inner area");
            }

            foreach (KeyValuePair<Direction, TerrainCell> pair in cell.Neighbors)
            {
                TerrainCell nCell = pair.Value;

                if (exploredSet.Contains(nCell)) continue;

                if (TerrainCell.IsDiagonalDirection(pair.Key)) continue;

                if (!IsCellEnclosed(nCell)) continue;

                toAdd.Enqueue(nCell);
                exploredSet.Add(nCell);
            }
        }

        if (cellSet.Area == 0)
        {
            return null;
        }

        cellSet.Update();

        return cellSet;
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
