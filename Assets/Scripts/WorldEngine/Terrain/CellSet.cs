using UnityEngine;
using System.Collections.Generic;

public delegate bool CanAddCellDelegate(TerrainCell cell);

public class CellSet : ICellSet
{
    public World World;

    public HashSet<TerrainCell> Cells = new HashSet<TerrainCell>();

    public TerrainCell Top;
    public TerrainCell Bottom;
    public TerrainCell Left;
    public TerrainCell Right;

    private int _top;
    private int _bottom;
    private int _left;
    private int _right;

    public int RectArea = 0;
    public int RectWidth = 0;
    public int RectHeight = 0;

    public int Area = 0;

    public bool NeedsUpdate = false;

    private bool _initialized = false;

    public CellArea GetArea()
    {
        CellArea area = new CellArea()
        {
            World = World,
            Cells = Cells
        };

        return area;
    }

    public void AddCell(TerrainCell cell)
    {
        if (!Cells.Add(cell)) return; // cell already present

        Area++;

        NeedsUpdate = true;

        if (!_initialized)
        {
            World = cell.World;

            Top = cell;
            Bottom = cell;
            Left = cell;
            Right = cell;

            _top = cell.Latitude;
            _bottom = cell.Latitude;
            _left = cell.Longitude;
            _right = cell.Longitude;

            _initialized = true;
            return;
        }

        if (cell.Latitude > _top)
        {
#if DEBUG
            if (Territory.DEBUG_territoryDebug)
            {
                Manager.Debug_BreakRequested = true;
            }
#endif
            Top = cell;
            _top = cell.Latitude;
        }

        if (cell.Latitude < _bottom)
        {
#if DEBUG
            if (Territory.DEBUG_territoryDebug)
            {
                Manager.Debug_BreakRequested = true;
            }
#endif
            Bottom = cell;
            _bottom = cell.Latitude;
        }

        if (cell.Longitude < _left)
        {
#if DEBUG
            if (Territory.DEBUG_territoryDebug)
            {
                Manager.Debug_BreakRequested = true;
            }
#endif
            int modLong = cell.Longitude + Manager.WorldWidth;
            int leftDiff = _left - cell.Longitude;
            int rightDiff = Mathf.Abs(_right - modLong);

            if (rightDiff < leftDiff)
            {
                if (modLong > _right)
                {
                    Right = cell;
                    _right = modLong;
                }
            }
            else
            {
                Left = cell;
                _left = cell.Longitude;
            }
        }

        if (cell.Longitude > _right)
        {
#if DEBUG
            if (Territory.DEBUG_territoryDebug)
            {
                Manager.Debug_BreakRequested = true;
            }
#endif
            Right = cell;
            _right = cell.Longitude;
        }
    }

    public void Update()
    {
        if (!_initialized)
        {
            throw new System.Exception("CellSet not initialized");
        }

        RectHeight = _top - _bottom + 1;
        RectWidth = _right - _left + 1;

        RectArea = RectWidth * RectHeight;

        Area = Cells.Count;

        NeedsUpdate = false;
    }

    public bool IsCellEnclosed(TerrainCell cell)
    {
        if (!cell.Latitude.IsInsideRange(_bottom, _top)) return false;

        if (cell.Longitude.IsInsideRange(_left, _right)) return true;

        return false;
    }

    public void Merge(CellSet sourceSet)
    {
        Cells.UnionWith(sourceSet.Cells);

        if (_top < sourceSet._top)
        {
            Top = sourceSet.Top;
            _top = sourceSet._top;
        }

        if (_bottom > sourceSet._bottom)
        {
            Bottom = sourceSet.Bottom;
            _bottom = sourceSet._bottom;
        }

        if (_left > sourceSet._left)
        {
            Left = sourceSet.Left;
            _left = sourceSet._left;
        }

        if (_right > sourceSet._right)
        {
            Right = sourceSet.Right;
            _right = sourceSet._right;
        }

        NeedsUpdate = true;
    }

    public static IEnumerable<CellSet> SplitIntoSubsets(
        CellSet cellSet,
        int maxMajorLength,
        int minMajorLength,
        float maxScaleDiff,
        float minRectAreaPercent)
    {
        int majorLength = Mathf.Max(cellSet.RectHeight, cellSet.RectWidth);
        int minorLength = Mathf.Min(cellSet.RectHeight, cellSet.RectWidth);
        float scaleDiff = majorLength / (float)minorLength;
        float rectAreaPercent = cellSet.Area / (float)cellSet.RectArea;

        bool noNeedToSplit = majorLength <= minMajorLength;

        noNeedToSplit |= (majorLength <= maxMajorLength) &&
            (scaleDiff < maxScaleDiff) &&
            (rectAreaPercent > minRectAreaPercent) &&
            cellSet.IsContiguous();

        if (noNeedToSplit)
        {
            yield return cellSet;
        }
        else if (majorLength == cellSet.RectHeight)
        {
            int middleLatitude = (cellSet.Top.Latitude + cellSet.Bottom.Latitude) / 2;

            CellSet topCellSet = new CellSet();
            CellSet bottomCellSet = new CellSet();

            foreach (TerrainCell cell in cellSet.Cells)
            {
                if (cell.Latitude < middleLatitude)
                    bottomCellSet.AddCell(cell);
                else
                    topCellSet.AddCell(cell);
            }

            topCellSet.Update();
            bottomCellSet.Update();

            foreach (CellSet subset in SplitIntoSubsets(
                topCellSet, maxMajorLength, minMajorLength, maxScaleDiff, minRectAreaPercent))
            {
                yield return subset;
            }

            foreach (CellSet subset in SplitIntoSubsets(
                bottomCellSet, maxMajorLength, minMajorLength, maxScaleDiff, minRectAreaPercent))
            {
                yield return subset;
            }
        }
        else
        {
            int middleLongitude = (cellSet.Left.Longitude + cellSet.Right.Longitude) / 2;

            CellSet leftCellSet = new CellSet();
            CellSet rightCellSet = new CellSet();

            foreach (TerrainCell cell in cellSet.Cells)
            {
                if (cell.Longitude > middleLongitude)
                    rightCellSet.AddCell(cell);
                else
                    leftCellSet.AddCell(cell);
            }

            leftCellSet.Update();
            rightCellSet.Update();

            foreach (CellSet subset in SplitIntoSubsets(
                leftCellSet, maxMajorLength, minMajorLength, maxScaleDiff, minRectAreaPercent))
            {
                yield return subset;
            }

            foreach (CellSet subset in SplitIntoSubsets(
                rightCellSet, maxMajorLength, minMajorLength, maxScaleDiff, minRectAreaPercent))
            {
                yield return subset;
            }
        }
    }

    public WorldPosition GetCentroid()
    {
        int centroidLongitude = 0, centroidLatitude = 0;

        foreach (TerrainCell cell in Cells)
        {
            int cellLongitude = cell.Longitude;

            if (cellLongitude < Left.Longitude)
            {
                // the cell has wrapped around the world
                cellLongitude += Manager.WorldWidth;
            }

            centroidLongitude += cellLongitude;
            centroidLatitude += cell.Latitude;
        }

        centroidLongitude /= Cells.Count;
        centroidLatitude /= Cells.Count;

        return new WorldPosition(centroidLongitude, centroidLatitude);
    }

    public TerrainCell GetMostCenteredCell()
    {
        WorldPosition centroid = GetCentroid();

        TerrainCell closestCell = null;
        int closestDistCenter = int.MaxValue;

        foreach (TerrainCell cell in Cells)
        {
            int cellLongitude = cell.Longitude;

            if (cellLongitude < Left.Longitude)
            {
                // the cell has wrapped around the world
                cellLongitude += Manager.WorldWidth;
            }

            int distCenter =
                Mathf.Abs(cellLongitude - centroid.Longitude) +
                Mathf.Abs(cell.Latitude - centroid.Latitude);

            if ((closestCell == null) || (distCenter < closestDistCenter))
            {
                closestDistCenter = distCenter;
                closestCell = cell;
            }
        }

        return closestCell;
    }

    public bool IsContiguous()
    {
        int connectedArea = 0;

        HashSet<TerrainCell> exploredCells = new HashSet<TerrainCell>();
        Queue<TerrainCell> cellsToExplore = new Queue<TerrainCell>();

        exploredCells.Add(Top);
        cellsToExplore.Enqueue(Top);

        while (cellsToExplore.Count > 0)
        {
            TerrainCell cell = cellsToExplore.Dequeue();

            connectedArea++;

            foreach (KeyValuePair<Direction, TerrainCell> pair in cell.NonDiagonalNeighbors)
            {
                TerrainCell nCell = pair.Value;

                if (exploredCells.Contains(nCell)) continue; // skip if already explored
                if (!Cells.Contains(nCell)) continue; // skip if not part of CellSet

                exploredCells.Add(nCell);
                cellsToExplore.Enqueue(nCell);
            }
        }

        return connectedArea == Area;
    }

    public ICollection<TerrainCell> GetCells()
    {
        return Cells;
    }

    public RectInt GetBoundingRectangle()
    {
        return new RectInt(_left, _bottom, _right - _left, _top - _bottom);
    }

    public static RectInt GetBoundingRectangle(ICollection<TerrainCell> cells)
    {
        int xMin = 0;
        int xMax = 0;
        int yMin = 0;
        int yMax = 0;
        bool first = true;

        foreach (var cell in cells)
        {
            var pos = cell.Position;

            if (first)
            {
                xMin = pos.Longitude;
                xMax = pos.Longitude;
                yMin = pos.Latitude;
                yMax = pos.Latitude;

                first = false;
                continue;
            }

            if (pos.Longitude < xMin)
            {
                int modLong = pos.Longitude + Manager.WorldWidth;
                int maxAltDiff = Mathf.Abs(xMax - modLong);
                int minDiff = xMin - pos.Longitude;

                if (maxAltDiff < minDiff)
                {
                    if (modLong > xMax)
                    {
                        xMax = modLong;
                    }
                }
                else
                {
                    xMin = pos.Longitude;
                }
            }

            xMax = Mathf.Max(xMax, pos.Longitude);
            yMin = Mathf.Min(yMin, pos.Latitude);
            yMax = Mathf.Max(yMax, pos.Latitude);
        }

        return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
    }
}
