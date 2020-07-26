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
        Cells = new HashSet<TerrainCell>();

        Cells.Add(startCell);

        Top = startCell;
        Bottom = startCell;
        Left = startCell;
        Right = startCell;
    }

    public void GetEnclosedCellSet(
        HashSet<TerrainCell> outsideSet,
        out HashSet<TerrainCell> set,
        out int area)
    {
        set = new HashSet<TerrainCell>();
        area = 0;

        HashSet<TerrainCell> exploredSet = new HashSet<TerrainCell>();
        exploredSet.UnionWith(outsideSet);

        Queue<TerrainCell> toAdd = new Queue<TerrainCell>();

        toAdd.Enqueue(Top);

        bool warn = true;

        while (toAdd.Count > 0)
        {
            TerrainCell cell = toAdd.Dequeue();

            if (!cell.IsLiquidSea)
            {
                set.Add(cell);
                area++;
            }

            if (warn && (area > RectArea))
            {
                Debug.LogWarning("enclosed set greater than rect area");
                warn = false;
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
    }
}
