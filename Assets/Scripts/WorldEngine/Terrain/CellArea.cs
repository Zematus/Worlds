using System.Collections.Generic;
using System.Xml.Serialization;

public class CellArea : ISynchronizable
{
    public List<WorldPosition> CellPositions;

    [XmlIgnore]
    public World World;

    [XmlIgnore]
    public HashSet<TerrainCell> Cells = new HashSet<TerrainCell>();

    public void FinalizeLoad()
    {
        foreach (WorldPosition position in CellPositions)
        {
            TerrainCell cell = World.GetCell(position);

            if (cell == null)
            {
                throw new System.Exception("Cell missing at position " + position.Longitude + "," + position.Latitude);
            }

            Cells.Add(cell);
        }
    }

    public void Synchronize()
    {
        CellPositions = new List<WorldPosition>(Cells.Count);

        foreach (TerrainCell cell in Cells)
        {
            CellPositions.Add(cell.Position);
        }
    }
}
