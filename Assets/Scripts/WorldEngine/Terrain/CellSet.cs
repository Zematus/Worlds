using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

public class CellSet
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

        if ((cell.Longitude - Left.Longitude) == -1)
        {
            Left = cell;
        }
        else if ((cell.Longitude - Left.Longitude - Manager.WorldWidth) == -1)
        {
            Left = cell;
            WrapsAround = true;
        }

        if ((cell.Longitude - Right.Longitude) == 1)
        {
            Right = cell;
        }
        else if ((cell.Longitude - Right.Longitude + Manager.WorldWidth) == 1)
        {
            Right = cell;
            WrapsAround = true;
        }

        if ((cell.Latitude - Top.Latitude) == -1)
        {
            Top = cell;
        }

        if ((cell.Latitude - Bottom.Latitude) == 1)
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

        RectHeight = bottom - top + 1;
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

        if (!cell.Latitude.IsInsideRange(top, bottom)) return false;

        int longitude = cell.Longitude;

        if (longitude.IsInsideRange(left, right)) return true;

        longitude += Manager.WorldWidth;

        if (longitude.IsInsideRange(left, right)) return true;

        return false;
    }

    public void Merge(CellSet sourceSet)
    {
        Cells.UnionWith(sourceSet.Cells);

        WrapsAround |= sourceSet.WrapsAround;

        if (Top.Latitude > sourceSet.Top.Latitude)
        {
            Top = sourceSet.Top;
        }

        if (Bottom.Latitude < sourceSet.Bottom.Latitude)
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

        NeedsUpdate = true;
    }

    public static IEnumerable<CellSet> SplitIntoSubsets(
        CellSet cellSet,
        int maxMajorLength,
        int minMajorLength,
        float maxScaleDiff,
        float minRectAreaPercent)
    {
        Debug.Log("Spliting set with area: " + cellSet.Area + ", width: " +
            cellSet.RectWidth + ", height: " + cellSet.RectHeight);

        int majorLength = Mathf.Max(cellSet.RectHeight, cellSet.RectWidth);
        int minorLength = Mathf.Min(cellSet.RectHeight, cellSet.RectWidth);
        float scaleDiff = majorLength / (float)minorLength;
        float rectAreaPercent = cellSet.Area / (float)cellSet.RectArea;
        float percentFactor = Mathf.Max(0, rectAreaPercent - minRectAreaPercent) / (1 - minRectAreaPercent);

        float acceptableMajorLength = Mathf.Lerp(minMajorLength, maxMajorLength, percentFactor);

        if ((majorLength <= acceptableMajorLength) ||
            ((majorLength <= maxMajorLength) &&
            (scaleDiff < maxScaleDiff)))
        {
            Debug.Log("Returning set with area: " + cellSet.Area);

            yield return cellSet;
        }
        else if (majorLength == cellSet.RectHeight)
        {
            int middleHeight = (cellSet.Top.Latitude + cellSet.Bottom.Latitude) / 2;

            CellSet topCellSet = new CellSet();
            CellSet bottomCellSet = new CellSet();

            foreach (TerrainCell cell in cellSet.Cells)
            {
                if (cell.Latitude > middleHeight)
                    bottomCellSet.AddCell(cell);
                else
                    topCellSet.AddCell(cell);
            }

            topCellSet.Update();
            bottomCellSet.Update();

            Debug.Log("topCellSet area: " + topCellSet.Area);
            Debug.Log("bottomCellSet area: " + bottomCellSet.Area);

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
            int middleWidth = (cellSet.Left.Longitude + cellSet.Right.Longitude) / 2;

            CellSet leftCellSet = new CellSet();
            CellSet rightCellSet = new CellSet();

            foreach (TerrainCell cell in cellSet.Cells)
            {
                if (cell.Longitude > middleWidth)
                    rightCellSet.AddCell(cell);
                else
                    leftCellSet.AddCell(cell);
            }

            leftCellSet.Update();
            rightCellSet.Update();

            Debug.Log("leftCellSet area: " + leftCellSet.Area);
            Debug.Log("rightCellSet area: " + rightCellSet.Area);

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
            centroidLongitude += cell.Longitude;
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
            int distCenter =
                Mathf.Abs(cell.Longitude - centroid.Longitude) +
                Mathf.Abs(cell.Latitude - centroid.Latitude);

            if ((closestCell == null) || (distCenter < closestDistCenter))
            {
                closestDistCenter = distCenter;
                closestCell = cell;
            }
        }

        return closestCell;
    }
}
