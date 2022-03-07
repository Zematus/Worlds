using System.Collections.Generic;
using UnityEngine;

public interface ICellSet
{
    ICollection<TerrainCell> GetCells();

    RectInt GetBoundingRectangle();
}
