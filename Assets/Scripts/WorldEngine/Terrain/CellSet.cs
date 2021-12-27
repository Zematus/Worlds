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

    public int RectArea = 0;
    public int RectWidth = 0;
    public int RectHeight = 0;

    public int Area = 0;

    public bool WrapsAround = false;

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

            _initialized = true;
            return;
        }

        int diffLeft = cell.Longitude - Left.Longitude;
        int diffRight = cell.Longitude - Right.Longitude;

        if (diffLeft < 0)
        {
            if ((diffRight + diffLeft + Manager.WorldWidth) < 0)
            {
                // Wrapped around, the cell would be closer to the 
                // rightmost cell and would be even more rightmost
                Right = cell;
                WrapsAround = true;
            }
            else
            {
                Left = cell;
            }
        }

        if (diffRight > 0)
        {
            if ((diffRight + diffLeft - Manager.WorldWidth) > 0)
            {
                // Wrapped around, the cell would be closer to the 
                // leftmost cell and would be even more leftmost
                Left = cell;
                WrapsAround = true;
            }
            else
            {
                Right = cell;
            }
        }

        if ((cell.Latitude - Top.Latitude) > 0)
        {
            Top = cell;
        }

        if ((cell.Latitude - Bottom.Latitude) < 0)
        {
            Bottom = cell;
        }
    }

    public void Update()
    {
        int top = Top.Latitude;
        int bottom = Bottom.Latitude;
        int left = Left.Longitude;
        int right = Right.Longitude;

        // adjust for world wrap
        if (WrapsAround) right += Manager.WorldWidth;

        RectHeight = top - bottom + 1;
        RectWidth = right - left + 1;

        RectArea = RectWidth * RectHeight;

        Area = Cells.Count;

        NeedsUpdate = false;
    }

    public bool IsCellEnclosed(TerrainCell cell)
    {
        int top = Top.Latitude;
        int bottom = Bottom.Latitude;
        int left = Left.Longitude;
        int right = Right.Longitude;

        // adjust for world wrap
        if (WrapsAround) right += Manager.WorldWidth;

        if (!cell.Latitude.IsInsideRange(bottom, top)) return false;

        int longitude = cell.Longitude;

        if (longitude.IsInsideRange(left, right)) return true;

        longitude += Manager.WorldWidth;

        if (longitude.IsInsideRange(left, right)) return true;

        return false;
    }

    public void Merge(CellSet sourceSet)
    {
        Cells.UnionWith(sourceSet.Cells);

        if (Top.Latitude < sourceSet.Top.Latitude)
        {
            Top = sourceSet.Top;
        }

        if (Bottom.Latitude > sourceSet.Bottom.Latitude)
        {
            Bottom = sourceSet.Bottom;
        }

        bool offsetNeeded = false;
        bool rightOffsetDone = false;
        bool sourceSetRightOffsetDone = false;

        int rigthLongitude = Right.Longitude;
        if (WrapsAround)
        {
            rigthLongitude += Manager.WorldWidth;
            offsetNeeded = true;
            rightOffsetDone = true;
        }

        int sourceSetRigthLongitude = sourceSet.Right.Longitude;
        if (sourceSet.WrapsAround)
        {
            sourceSetRigthLongitude += Manager.WorldWidth;
            offsetNeeded = true;
            sourceSetRightOffsetDone = true;
        }

        int leftLongitude = Left.Longitude;
        if (offsetNeeded && !rightOffsetDone)
        {
            rigthLongitude += Manager.WorldWidth;
            leftLongitude += Manager.WorldWidth;
        }

        int sourceSetLeftLongitude = sourceSet.Left.Longitude;
        if (offsetNeeded && !sourceSetRightOffsetDone)
        {
            sourceSetRigthLongitude += Manager.WorldWidth;
            sourceSetLeftLongitude += Manager.WorldWidth;
        }

        if (leftLongitude > sourceSetLeftLongitude)
        {
            Left = sourceSet.Left;
        }

        if (rigthLongitude < sourceSetRigthLongitude)
        {
            Right = sourceSet.Right;
        }

        WrapsAround |= sourceSet.WrapsAround;

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
        int xMin = Left.Longitude;
        int xMax = Right.Longitude;
        int yMin = Bottom.Latitude;
        int yMax = Top.Latitude;

        // this makes sure the rectagle can correctly wrap around the vertical edges of the map if needed
        if (xMax < xMin)
        {
            xMax += World.Width;
        }

        return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
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
                int altLong = pos.Longitude + Manager.WorldWidth;
                int maxAltDiff = Mathf.Abs(xMax - altLong);
                int minDiff = Mathf.Abs(xMin - pos.Longitude);

                if (maxAltDiff < minDiff)
                {
                    if (altLong > xMax)
                    {
                        xMax = pos.Longitude;
                    }
                }
                else
                {
                    xMin = pos.Longitude;
                }
            }
            if (pos.Longitude > xMax)
            {
                int altLong = pos.Longitude - Manager.WorldWidth;
                int minAltDiff = Mathf.Abs(xMin - altLong);
                int maxDiff = Mathf.Abs(xMax - pos.Longitude);

                if (minAltDiff < maxDiff)
                {
                    if (altLong < xMin)
                    {
                        xMin = pos.Longitude;
                    }
                }
                else
                {
                    xMax = pos.Longitude;
                }
            }
            if (pos.Latitude < yMin)
            {
                yMin = pos.Latitude;
            }
            if (pos.Latitude > yMax)
            {
                yMax = pos.Latitude;
            }
        }

        // this makes sure the rectagle can correctly wrap around the vertical edges of the map if needed
        if (xMax < xMin)
        {
            xMax += Manager.WorldWidth;
        }

        return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
    }
}
